using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.PlanosBiblicos;

public sealed class PlanoBiblicoService : IPlanoBiblicoService
{
    private readonly IPlanoBiblicoRepository _planos;
    private readonly IBiblePlanGeneratorService _generator;
    private readonly IUnitOfWork _unitOfWork;

    public PlanoBiblicoService(IPlanoBiblicoRepository planos, IBiblePlanGeneratorService generator, IUnitOfWork unitOfWork)
    {
        _planos = planos;
        _generator = generator;
        _unitOfWork = unitOfWork;
    }

    public async Task<PlanoBiblicoResponse?> GetAtivoAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var plano = await _planos.GetAtivoAsync(usuarioId, cancellationToken);
        return plano is null ? null : ToResponse(plano);
    }

    public async Task<PlanoBiblicoResponse> CriarAsync(Guid usuarioId, CriarPlanoBiblicoRequest request, CancellationToken cancellationToken = default)
    {
        PlanoBiblicoUsuario? plano = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var ativo = await _planos.GetAtivoAsync(usuarioId, cancellationToken);
            if (ativo is not null)
            {
                throw new AppException("Usuario ja possui plano biblico ativo.");
            }

            plano = await _generator.GenerateAsync(usuarioId, request.Nome, request.DuracaoAnos, request.DuracaoMeses, request.DataInicio, ModoCriacaoPlanoBiblico.NovoDoInicio, 1, null, cancellationToken);
            _planos.Add(plano);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return ToResponse(plano!);
    }

    public async Task<PlanoBiblicoResponse> AlterarAsync(Guid usuarioId, AlterarPlanoBiblicoRequest request, CancellationToken cancellationToken = default)
    {
        PlanoBiblicoUsuario? novoPlano = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var planoAtual = await _planos.GetAtivoAsync(usuarioId, cancellationToken)
                ?? throw new AppException("Plano biblico ativo nao encontrado.", 404);
            var posicao = await _planos.GetPosicaoAsync(usuarioId, cancellationToken);
            var continuar = request.Modo == ModoAlteracaoPlanoBiblico.ContinuarDeOndeParei;
            var ordemInicio = continuar ? (posicao?.UltimaOrdemConcluida ?? 0) + 1 : 1;
            var modo = continuar ? ModoCriacaoPlanoBiblico.Continuacao : ModoCriacaoPlanoBiblico.Reinicio;
            novoPlano = await _generator.GenerateAsync(usuarioId, planoAtual.Nome, request.DuracaoAnos, request.DuracaoMeses, DateOnly.FromDateTime(DateTime.UtcNow), modo, ordemInicio, planoAtual.Id, cancellationToken);

            planoAtual.Ativo = false;
            planoAtual.Status = StatusPlanoBiblico.Substituido;
            planoAtual.AtualizadoEm = DateTime.UtcNow;

            _planos.Add(novoPlano);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return ToResponse(novoPlano!);
    }

    public async Task<PlanoBiblicoResponse> GetAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default) =>
        ToResponse(await _planos.GetByIdAsync(planoId, usuarioId, cancellationToken) ?? throw new AppException("Plano biblico nao encontrado.", 404));

    public async Task<IReadOnlyList<PlanoBiblicoResponse>> GetHistoricoAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        (await _planos.ListHistoricoAsync(usuarioId, cancellationToken)).Select(ToResponse).ToList();

    public async Task<IReadOnlyList<PlanoBiblicoDiaResponse>> ListDiasAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default) =>
        (await _planos.ListDiasAsync(planoId, usuarioId, cancellationToken)).Select(d => new PlanoBiblicoDiaResponse(d.Id, d.DiaNumero, d.MesNumero, d.DataPrevista, d.LeiturasTexto, d.SalmoNumero)).ToList();

    public async Task<PosicaoBiblicaResponse> GetPosicaoAtualAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var posicao = await _planos.GetPosicaoAsync(usuarioId, cancellationToken);
        return new PosicaoBiblicaResponse(posicao?.UltimaOrdemConcluida ?? 0, posicao?.UltimaBaseBiblicaConcluidaId);
    }

    public async Task ConcluirDiaAsync(Guid usuarioId, Guid diaId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var dia = await _planos.GetDiaAsync(diaId, usuarioId, cancellationToken)
                ?? throw new AppException("Dia do plano nao encontrado.", 404);
            var now = DateTime.UtcNow;
            var progresso = await _planos.GetProgressoLeituraAsync(usuarioId, dia.Id, cancellationToken);

            if (progresso is null)
            {
                _planos.Add(new ProgressoLeitura
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    PlanoBiblicoDiaId = dia.Id,
                    Concluido = true,
                    ConcluidoEm = now,
                    CriadoEm = now
                });
            }
            else
            {
                progresso.Concluido = true;
                progresso.ConcluidoEm = now;
            }

            var ultimoCapitulo = dia.Capitulos.OrderByDescending(c => c.Ordem).FirstOrDefault();
            var posicao = await _planos.GetPosicaoAsync(usuarioId, cancellationToken);
            if (posicao is null)
            {
                _planos.Add(new PosicaoBiblicaUsuario
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    UltimaOrdemConcluida = ultimoCapitulo?.Ordem ?? 0,
                    UltimaBaseBiblicaConcluidaId = ultimoCapitulo?.BaseBiblicaId,
                    AtualizadoEm = now
                });
            }
            else if ((ultimoCapitulo?.Ordem ?? 0) >= posicao.UltimaOrdemConcluida)
            {
                posicao.UltimaOrdemConcluida = ultimoCapitulo?.Ordem ?? posicao.UltimaOrdemConcluida;
                posicao.UltimaBaseBiblicaConcluidaId = ultimoCapitulo?.BaseBiblicaId ?? posicao.UltimaBaseBiblicaConcluidaId;
                posicao.AtualizadoEm = now;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    private static PlanoBiblicoResponse ToResponse(PlanoBiblicoUsuario plano) =>
        new(plano.Id, plano.Nome, plano.DuracaoDias, plano.DuracaoAnos, plano.DuracaoMeses, plano.DataInicio, plano.DataFimPrevista, plano.Status.ToString(), plano.OrdemInicio, plano.OrdemFim, plano.ModoCriacao.ToString());
}
