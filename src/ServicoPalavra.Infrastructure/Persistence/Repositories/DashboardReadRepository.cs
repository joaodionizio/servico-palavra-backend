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

    public async Task<DashboardMeResponse> GetMeAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var favoritos = await CountFavoritosAsync(usuarioId, cancellationToken);
        var concluidos = await CountConteudosConcluidosAsync(usuarioId, cancellationToken);
        var formacoes = await ListFormacoesRecentesAsync(cancellationToken);
        var favoritosRecentes = await ListFavoritosRecentesAsync(usuarioId, cancellationToken);
        var conteudosComProgresso = await ListConteudosComProgressoAsync(usuarioId, cancellationToken);
        var plano = await GetPlanoBiblicoAtivoAsync(usuarioId, cancellationToken);
        var leituraHoje = plano is null ? null : await GetLeituraHojeAsync(usuarioId, plano.Id, cancellationToken);
        var percentualPlano = plano?.Percentual ?? 0m;

        return new DashboardMeResponse(
            concluidos,
            favoritos,
            plano is not null,
            formacoes,
            favoritosRecentes,
            conteudosComProgresso,
            plano,
            leituraHoje,
            new DashboardEstatisticasResponse(favoritos, concluidos, percentualPlano));
    }

    private Task<int> CountFavoritosAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        _db.Favoritos.AsNoTracking().CountAsync(x => x.UsuarioId == usuarioId && x.Conteudo.Publicado, cancellationToken);

    private Task<int> CountConteudosConcluidosAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        _db.ProgressosConteudo
            .AsNoTracking()
            .CountAsync(x => x.UsuarioId == usuarioId && x.Conteudo.Publicado && x.Status == StatusProgressoConteudo.Concluido, cancellationToken);

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

    private async Task<DashboardPlanoBiblicoResumoResponse?> GetPlanoBiblicoAtivoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var plano = await _db.PlanosBiblicosUsuario
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Ativo && x.Status == StatusPlanoBiblico.Ativo)
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.DuracaoDias,
                x.DataInicio,
                x.DataFimPrevista,
                TotalDias = _db.PlanosBiblicosDias.Count(d => d.PlanoBiblicoUsuarioId == x.Id),
                DiasConcluidos = _db.ProgressosLeitura.Count(p =>
                    p.UsuarioId == usuarioId &&
                    p.Concluido &&
                    p.PlanoBiblicoDia.PlanoBiblicoUsuarioId == x.Id)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (plano is null)
        {
            return null;
        }

        var percentual = plano.TotalDias == 0 ? 0m : Math.Round(plano.DiasConcluidos * 100m / plano.TotalDias, 2);
        return new DashboardPlanoBiblicoResumoResponse(
            plano.Id,
            plano.Nome,
            plano.DuracaoDias,
            plano.DataInicio,
            plano.DataFimPrevista,
            plano.DiasConcluidos,
            plano.TotalDias,
            percentual);
    }

    private async Task<DashboardLeituraHojeResponse?> GetLeituraHojeAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var leitura = await GetLeituraByDataAsync(usuarioId, planoId, hoje, cancellationToken)
            ?? await GetProximaLeituraPendenteAsync(usuarioId, planoId, cancellationToken);

        return leitura;
    }

    private Task<DashboardLeituraHojeResponse?> GetLeituraByDataAsync(Guid usuarioId, Guid planoId, DateOnly data, CancellationToken cancellationToken) =>
        _db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == planoId && x.DataPrevista == data)
            .Select(x => new DashboardLeituraHojeResponse(
                x.Id,
                x.DiaNumero,
                x.MesNumero,
                x.DataPrevista,
                x.LeiturasTexto,
                x.SalmoNumero,
                _db.ProgressosLeitura.Any(p => p.UsuarioId == usuarioId && p.PlanoBiblicoDiaId == x.Id && p.Concluido)))
            .FirstOrDefaultAsync(cancellationToken);

    private Task<DashboardLeituraHojeResponse?> GetProximaLeituraPendenteAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken) =>
        _db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == planoId &&
                !_db.ProgressosLeitura.Any(p => p.UsuarioId == usuarioId && p.PlanoBiblicoDiaId == x.Id && p.Concluido))
            .OrderBy(x => x.DiaNumero)
            .Select(x => new DashboardLeituraHojeResponse(
                x.Id,
                x.DiaNumero,
                x.MesNumero,
                x.DataPrevista,
                x.LeiturasTexto,
                x.SalmoNumero,
                false))
            .FirstOrDefaultAsync(cancellationToken);
}
