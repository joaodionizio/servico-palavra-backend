using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.Progresso;

public sealed class ProgressoService : IProgressoService
{
    private readonly IConteudoRepository _conteudos;
    private readonly IProgressoRepository _progressos;
    private readonly IUnitOfWork _unitOfWork;

    public ProgressoService(IConteudoRepository conteudos, IProgressoRepository progressos, IUnitOfWork unitOfWork)
    {
        _conteudos = conteudos;
        _progressos = progressos;
        _unitOfWork = unitOfWork;
    }

    public async Task ConcluirConteudoAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default)
    {
        _ = await _conteudos.GetByIdAsync(conteudoId, false, cancellationToken) ?? throw new AppException("Conteudo nao encontrado.", 404);
        var now = DateTime.UtcNow;
        var progresso = await _progressos.GetConteudoAsync(usuarioId, conteudoId, cancellationToken);

        if (progresso is null)
        {
            _progressos.Add(new ProgressoConteudo
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                ConteudoId = conteudoId,
                Status = StatusProgressoConteudo.Concluido,
                Percentual = 100,
                IniciadoEm = now,
                ConcluidoEm = now,
                UltimoAcessoEm = now,
                CriadoEm = now
            });
        }
        else
        {
            progresso.Status = StatusProgressoConteudo.Concluido;
            progresso.Percentual = 100;
            progresso.ConcluidoEm ??= now;
            progresso.UltimoAcessoEm = now;
            progresso.AtualizadoEm = now;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DesmarcarConclusaoConteudoAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default)
    {
        _ = await _conteudos.GetByIdAsync(conteudoId, false, cancellationToken) ?? throw new AppException("Conteudo nao encontrado.", 404);
        var progresso = await _progressos.GetConteudoAsync(usuarioId, conteudoId, cancellationToken);
        if (progresso is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        progresso.Status = StatusProgressoConteudo.NaoIniciado;
        progresso.Percentual = 0;
        progresso.ConcluidoEm = null;
        progresso.UltimoAcessoEm = now;
        progresso.AtualizadoEm = now;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
