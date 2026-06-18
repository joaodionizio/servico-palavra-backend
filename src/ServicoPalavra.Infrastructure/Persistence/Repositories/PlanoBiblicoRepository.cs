using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.PlanosBiblicos;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class PlanoBiblicoRepository : IPlanoBiblicoRepository
{
    private readonly AppDbContext _db;
    public PlanoBiblicoRepository(AppDbContext db) => _db = db;
    public Task<PlanoBiblicoUsuario?> GetAtivoAsync(Guid usuarioId, CancellationToken cancellationToken = default) => _db.PlanosBiblicosUsuario.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.Ativo && x.Status == Domain.Enums.StatusPlanoBiblico.Ativo, cancellationToken);
    public Task<PlanoBiblicoUsuario?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default) => _db.PlanosBiblicosUsuario.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId, cancellationToken);
    public async Task<IReadOnlyList<PlanoBiblicoDia>> ListDiasAsync(Guid planoId, Guid usuarioId, CancellationToken cancellationToken = default) => await _db.PlanosBiblicosDias.AsNoTracking().Where(x => x.PlanoBiblicoUsuarioId == planoId && x.PlanoBiblicoUsuario.UsuarioId == usuarioId).OrderBy(x => x.DiaNumero).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<PlanoBiblicoDiaResponse>> ListDiaResponsesAsync(Guid planoId, Guid usuarioId, CancellationToken cancellationToken = default) => await _db.PlanosBiblicosDias
        .AsNoTracking()
        .Where(x => x.PlanoBiblicoUsuarioId == planoId && x.PlanoBiblicoUsuario.UsuarioId == usuarioId)
        .OrderBy(x => x.DiaNumero)
        .Select(x => new PlanoBiblicoDiaResponse(
            x.Id,
            x.DiaNumero,
            x.MesNumero,
            x.DataPrevista,
            x.LeiturasTexto,
            x.SalmoNumero,
            _db.ProgressosLeitura.Any(p => p.UsuarioId == usuarioId && p.PlanoBiblicoDiaId == x.Id && p.Concluido)))
        .ToListAsync(cancellationToken);
    public Task<PlanoBiblicoDiaResponse?> GetDiaResponseAsync(Guid diaId, Guid usuarioId, CancellationToken cancellationToken = default) => _db.PlanosBiblicosDias
        .AsNoTracking()
        .Where(x => x.Id == diaId && x.PlanoBiblicoUsuario.UsuarioId == usuarioId)
        .Select(x => new PlanoBiblicoDiaResponse(
            x.Id,
            x.DiaNumero,
            x.MesNumero,
            x.DataPrevista,
            x.LeiturasTexto,
            x.SalmoNumero,
            _db.ProgressosLeitura.Any(p => p.UsuarioId == usuarioId && p.PlanoBiblicoDiaId == x.Id && p.Concluido)))
        .FirstOrDefaultAsync(cancellationToken);
    public Task<PlanoBiblicoDia?> GetDiaAsync(Guid diaId, Guid usuarioId, CancellationToken cancellationToken = default) => _db.PlanosBiblicosDias.AsNoTracking().FirstOrDefaultAsync(x => x.Id == diaId && x.PlanoBiblicoUsuario.UsuarioId == usuarioId, cancellationToken);
    public Task<PosicaoBiblicaUsuario?> GetPosicaoAsync(Guid usuarioId, CancellationToken cancellationToken = default) => _db.PosicoesBiblicasUsuario.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);
    public async Task<(int Ordem, Guid? BaseBiblicaId)> GetUltimaPosicaoConcluidaAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var ultimo = await _db.ProgressosLeitura
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId
                && x.Concluido
                && x.PlanoBiblicoDia.PlanoBiblicoUsuario.UsuarioId == usuarioId
                && x.PlanoBiblicoDia.PlanoBiblicoUsuario.Ativo
                && x.PlanoBiblicoDia.PlanoBiblicoUsuario.Status == Domain.Enums.StatusPlanoBiblico.Ativo)
            .SelectMany(x => x.PlanoBiblicoDia.Capitulos)
            .OrderByDescending(x => x.Ordem)
            .Select(x => new { x.Ordem, x.BaseBiblicaId })
            .FirstOrDefaultAsync(cancellationToken);

        return ultimo is null ? (0, null) : (ultimo.Ordem, ultimo.BaseBiblicaId);
    }
    public async Task<IReadOnlyList<BaseBiblica>> ListBaseAsync(int ordemInicio, CancellationToken cancellationToken = default) => await _db.BaseBiblica.AsNoTracking().Where(x => x.Ativo && x.Ordem >= ordemInicio).OrderBy(x => x.Ordem).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<BaseBiblica>> ListBaseAtivaAsync(CancellationToken cancellationToken = default) => await _db.BaseBiblica.Where(x => x.Ativo).OrderBy(x => x.Ordem).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<PlanoBiblicoUsuario>> ListHistoricoAsync(Guid usuarioId, CancellationToken cancellationToken = default) => await _db.PlanosBiblicosUsuario.AsNoTracking().Where(x => x.UsuarioId == usuarioId).OrderByDescending(x => x.CriadoEm).ToListAsync(cancellationToken);
    public Task<ProgressoLeitura?> GetProgressoLeituraAsync(Guid usuarioId, Guid diaId, CancellationToken cancellationToken = default) => _db.ProgressosLeitura.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PlanoBiblicoDiaId == diaId, cancellationToken);
    public void Add(PlanoBiblicoUsuario plano) => _db.PlanosBiblicosUsuario.Add(plano);
    public void Add(ProgressoLeitura progresso) => _db.ProgressosLeitura.Add(progresso);
    public void Add(PosicaoBiblicaUsuario posicao) => _db.PosicoesBiblicasUsuario.Add(posicao);
}
