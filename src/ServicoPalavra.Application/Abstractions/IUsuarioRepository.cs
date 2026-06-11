using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Abstractions;

public interface IUsuarioRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
