namespace ServicoPalavra.Application.PlanosBiblicos;

public interface IPlanoBiblicoCalendarService
{
    Task<bool> RealignForReadAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default);
    Task<bool> RealignAfterCompletionAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default);
    Task<bool> RealignAfterReopenAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default);
}
