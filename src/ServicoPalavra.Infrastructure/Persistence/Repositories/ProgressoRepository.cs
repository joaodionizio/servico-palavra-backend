using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class ProgressoRepository : IProgressoRepository
{
    private readonly AppDbContext _db;
    public ProgressoRepository(AppDbContext db) => _db = db;
    public Task<ProgressoConteudo?> GetConteudoAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default) => _db.ProgressosConteudo.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.ConteudoId == conteudoId, cancellationToken);
    public void Add(ProgressoConteudo progresso) => _db.ProgressosConteudo.Add(progresso);
}
