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
        await _planos.ListDiaResponsesAsync(planoId, usuarioId, cancellationToken);

    public async Task<PosicaoBiblicaResponse> GetPosicaoAtualAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var posicao = await _planos.GetPosicaoAsync(usuarioId, cancellationToken);
        return new PosicaoBiblicaResponse(posicao?.UltimaOrdemConcluida ?? 0, posicao?.UltimaBaseBiblicaConcluidaId);
    }

    public async Task<PlanoBiblicoDiaResponse> ConcluirDiaAsync(Guid usuarioId, Guid diaId, CancellationToken cancellationToken = default) =>
        await AlterarConclusaoDiaAsync(usuarioId, diaId, concluido: true, cancellationToken);

    public async Task<PlanoBiblicoDiaResponse> DesmarcarDiaAsync(Guid usuarioId, Guid diaId, CancellationToken cancellationToken = default) =>
        await AlterarConclusaoDiaAsync(usuarioId, diaId, concluido: false, cancellationToken);

    private async Task<PlanoBiblicoDiaResponse> AlterarConclusaoDiaAsync(Guid usuarioId, Guid diaId, bool concluido, CancellationToken cancellationToken)
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
                    Concluido = concluido,
                    ConcluidoEm = concluido ? now : null,
                    CriadoEm = now
                });
            }
            else
            {
                progresso.Concluido = concluido;
                progresso.ConcluidoEm = concluido ? now : null;
                progresso.AtualizadoEm = now;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await RecalcularPosicaoBiblicaAsync(usuarioId, now, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return await _planos.GetDiaResponseAsync(diaId, usuarioId, cancellationToken)
            ?? throw new AppException("Dia do plano nao encontrado.", 404);
    }

    private async Task RecalcularPosicaoBiblicaAsync(Guid usuarioId, DateTime now, CancellationToken cancellationToken)
    {
        var ultimaPosicao = await _planos.GetUltimaPosicaoConcluidaAsync(usuarioId, cancellationToken);
        var posicao = await _planos.GetPosicaoAsync(usuarioId, cancellationToken);

        if (posicao is null)
        {
            _planos.Add(new PosicaoBiblicaUsuario
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                UltimaOrdemConcluida = ultimaPosicao.Ordem,
                UltimaBaseBiblicaConcluidaId = ultimaPosicao.BaseBiblicaId,
                AtualizadoEm = now
            });

            return;
        }

        posicao.UltimaOrdemConcluida = ultimaPosicao.Ordem;
        posicao.UltimaBaseBiblicaConcluidaId = ultimaPosicao.BaseBiblicaId;
        posicao.AtualizadoEm = now;
    }

    private static PlanoBiblicoResponse ToResponse(PlanoBiblicoUsuario plano) =>
        new(plano.Id, plano.Nome, plano.DuracaoDias, plano.DuracaoAnos, plano.DuracaoMeses, plano.DataInicio, plano.DataFimPrevista, plano.Status.ToString(), plano.OrdemInicio, plano.OrdemFim, plano.ModoCriacao.ToString());
}
