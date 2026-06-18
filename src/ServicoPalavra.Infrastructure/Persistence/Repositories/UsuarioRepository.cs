using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;
    public UsuarioRepository(AppDbContext db) => _db = db;
    public Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
}
