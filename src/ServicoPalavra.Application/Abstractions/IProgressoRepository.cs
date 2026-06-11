using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Abstractions;

public interface IProgressoRepository
{
    Task<ProgressoConteudo?> GetConteudoAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default);
    void Add(ProgressoConteudo progresso);
}
