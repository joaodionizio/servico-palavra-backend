using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Dashboard;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class DashboardReadRepository : IDashboardReadRepository
{
    private const int Limit = 5;
    private readonly AppDbContext _db;

    public DashboardReadRepository(AppDbContext db) => _db = db;

    public async Task<DashboardReadResult> GetMeAsync(Guid usuarioId, DateOnly today, CancellationToken cancellationToken = default)
    {
        var overview = await GetOverviewAsync(usuarioId, today, cancellationToken);
        var formacoes = await ListFormacoesRecentesAsync(cancellationToken);
        var favoritosRecentes = await ListFavoritosRecentesAsync(usuarioId, cancellationToken);
        var conteudosComProgresso = await ListConteudosComProgressoAsync(usuarioId, cancellationToken);
        var leituraHoje = overview.Plano is null
            ? null
            : await GetLeituraHojeAsync(usuarioId, overview.Plano.Id, today, cancellationToken);
        var percentualPlano = overview.Plano?.Percentual ?? 0m;

        var response = new DashboardMeResponse(
            overview.Concluidos,
            overview.Favoritos,
            overview.Plano is not null,
            formacoes,
            favoritosRecentes,
            conteudosComProgresso,
            overview.Plano,
            leituraHoje,
            new DashboardEstatisticasResponse(overview.Favoritos, overview.Concluidos, percentualPlano));

        return new DashboardReadResult(response, overview.NeedsCalendarRealignment);
    }

    private async Task<DashboardOverview> GetOverviewAsync(Guid usuarioId, DateOnly today, CancellationToken cancellationToken)
    {
        var row = await (
            from usuario in _db.Users.AsNoTracking()
            where usuario.Id == usuarioId
            join plano in _db.PlanosBiblicosUsuario
                    .AsNoTracking()
                    .Where(x => x.Ativo && x.Status == StatusPlanoBiblico.Ativo)
                on usuario.Id equals plano.UsuarioId into planos
            from plano in planos.DefaultIfEmpty()
            select new
            {
                Favoritos = _db.Favoritos.Count(x => x.UsuarioId == usuarioId && x.Conteudo.Publicado),
                Concluidos = _db.ProgressosConteudo.Count(x =>
                    x.UsuarioId == usuarioId &&
                    x.Conteudo.Publicado &&
                    x.Status == StatusProgressoConteudo.Concluido),
                PlanoId = plano == null ? (Guid?)null : plano.Id,
                PlanoNome = plano == null ? null : plano.Nome,
                PlanoDuracaoDias = plano == null ? (int?)null : plano.DuracaoDias,
                PlanoDataInicio = plano == null ? (DateOnly?)null : plano.DataInicio,
                PlanoDataFimPrevista = plano == null ? (DateOnly?)null : plano.DataFimPrevista,
                TotalDias = plano == null
                    ? 0
                    : _db.PlanosBiblicosDias.Count(d => d.PlanoBiblicoUsuarioId == plano.Id),
                DiasConcluidos = plano == null
                    ? 0
                    : _db.ProgressosLeitura.Count(p =>
                        p.UsuarioId == usuarioId &&
                        p.Concluido &&
                        p.PlanoBiblicoDia.PlanoBiblicoUsuarioId == plano.Id),
                NeedsCalendarRealignment = plano != null && _db.PlanosBiblicosDias.Any(d =>
                    d.PlanoBiblicoUsuarioId == plano.Id &&
                    (!d.DataPrevista.HasValue || d.DataPrevista.Value < today) &&
                    !_db.ProgressosLeitura.Any(p =>
                        p.UsuarioId == usuarioId &&
                        p.PlanoBiblicoDiaId == d.Id &&
                        p.Concluido))
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return new DashboardOverview(0, 0, null, false);
        }

        DashboardPlanoBiblicoResumoResponse? planoResponse = null;
        if (row.PlanoId.HasValue)
        {
            var percentual = row.TotalDias == 0
                ? 0m
                : Math.Round(row.DiasConcluidos * 100m / row.TotalDias, 2);

            planoResponse = new DashboardPlanoBiblicoResumoResponse(
                row.PlanoId.Value,
                row.PlanoNome ?? "Plano biblico",
                row.PlanoDuracaoDias ?? 0,
                row.PlanoDataInicio ?? default,
                row.PlanoDataFimPrevista ?? default,
                row.DiasConcluidos,
                row.TotalDias,
                percentual);
        }

        return new DashboardOverview(row.Favoritos, row.Concluidos, planoResponse, row.NeedsCalendarRealignment);
    }

    private async Task<IReadOnlyList<DashboardConteudoResumoResponse>> ListFormacoesRecentesAsync(CancellationToken cancellationToken) =>
        (await _db.Conteudos
            .AsNoTracking()
            .Where(x => x.Publicado)
            .OrderByDescending(x => x.PublicadoEm ?? x.CriadoEm)
            .ThenBy(x => x.Titulo)
            .Take(Limit)
            .Select(x => new
            {
                x.Id,
                x.Titulo,
                x.Slug,
                x.Tipo,
                x.Origem,
                x.Url,
                x.UrlThumbnail,
                x.DuracaoMinutos,
                Categoria = x.CategoriaConteudo == null ? null : x.CategoriaConteudo.Nome,
                x.PublicadoEm
            })
            .ToListAsync(cancellationToken))
        .Select(x => new DashboardConteudoResumoResponse(
            x.Id,
            x.Titulo,
            x.Slug,
            x.Tipo.ToString(),
            x.Origem.ToString(),
            x.Url,
            x.UrlThumbnail,
            x.DuracaoMinutos,
            x.Categoria,
            x.PublicadoEm))
        .ToList();

    private async Task<IReadOnlyList<DashboardFavoritoResponse>> ListFavoritosRecentesAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        (await _db.Favoritos
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Conteudo.Publicado)
            .OrderByDescending(x => x.CriadoEm)
            .Take(Limit)
            .Select(x => new
            {
                x.ConteudoId,
                x.Conteudo.Titulo,
                x.Conteudo.Slug,
                x.Conteudo.Tipo,
                x.Conteudo.Origem,
                x.Conteudo.Url,
                x.Conteudo.UrlThumbnail,
                x.CriadoEm
            })
            .ToListAsync(cancellationToken))
        .Select(x => new DashboardFavoritoResponse(
            x.ConteudoId,
            x.Titulo,
            x.Slug,
            x.Tipo.ToString(),
            x.Origem.ToString(),
            x.Url,
            x.UrlThumbnail,
            x.CriadoEm))
        .ToList();

    private async Task<IReadOnlyList<DashboardProgressoConteudoResponse>> ListConteudosComProgressoAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        (await _db.ProgressosConteudo
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Conteudo.Publicado && x.Status != StatusProgressoConteudo.NaoIniciado)
            .OrderByDescending(x => x.UltimoAcessoEm ?? x.AtualizadoEm ?? x.CriadoEm)
            .ThenBy(x => x.Conteudo.Titulo)
            .Take(Limit)
            .Select(x => new
            {
                x.ConteudoId,
                x.Conteudo.Titulo,
                x.Conteudo.Slug,
                x.Conteudo.Tipo,
                x.Conteudo.Origem,
                x.Conteudo.Url,
                x.Status,
                x.Percentual,
                x.UltimoAcessoEm
            })
            .ToListAsync(cancellationToken))
        .Select(x => new DashboardProgressoConteudoResponse(
            x.ConteudoId,
            x.Titulo,
            x.Slug,
            x.Tipo.ToString(),
            x.Origem.ToString(),
            x.Url,
            x.Status.ToString(),
            x.Percentual,
            x.UltimoAcessoEm))
        .ToList();

    private Task<DashboardLeituraHojeResponse?> GetLeituraHojeAsync(Guid usuarioId, Guid planoId, DateOnly today, CancellationToken cancellationToken) =>
        _db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x =>
                x.PlanoBiblicoUsuarioId == planoId &&
                (x.DataPrevista == today ||
                    !_db.ProgressosLeitura.Any(p =>
                        p.UsuarioId == usuarioId &&
                        p.PlanoBiblicoDiaId == x.Id &&
                        p.Concluido)))
            .OrderByDescending(x => x.DataPrevista == today)
            .ThenBy(x => x.DiaNumero)
            .Select(x => new DashboardLeituraHojeResponse(
                x.Id,
                x.DiaNumero,
                x.MesNumero,
                x.DataPrevista,
                x.LeiturasTexto,
                x.SalmoNumero,
                _db.ProgressosLeitura.Any(p =>
                    p.UsuarioId == usuarioId &&
                    p.PlanoBiblicoDiaId == x.Id &&
                    p.Concluido)))
            .FirstOrDefaultAsync(cancellationToken);

    private sealed record DashboardOverview(
        int Favoritos,
        int Concluidos,
        DashboardPlanoBiblicoResumoResponse? Plano,
        bool NeedsCalendarRealignment);
}
