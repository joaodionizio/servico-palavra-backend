namespace ServicoPalavra.Application.Progresso;

public interface IProgressoService
{
    Task ConcluirConteudoAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default);
}
