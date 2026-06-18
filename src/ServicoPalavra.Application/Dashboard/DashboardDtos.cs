namespace ServicoPalavra.Application.Dashboard;

public sealed record DashboardMeResponse(
    int ConteudosConcluidos,
    int Favoritos,
    bool PossuiPlanoBiblicoAtivo,
    IReadOnlyList<DashboardConteudoResumoResponse> FormacoesRecentes,
    IReadOnlyList<DashboardFavoritoResponse> FavoritosRecentes,
    IReadOnlyList<DashboardProgressoConteudoResponse> ConteudosComProgresso,
    DashboardPlanoBiblicoResumoResponse? PlanoBiblicoAtivo,
    DashboardLeituraHojeResponse? LeituraHoje,
    DashboardEstatisticasResponse Estatisticas);

public sealed record DashboardConteudoResumoResponse(
    Guid Id,
    string Titulo,
    string Slug,
    string Tipo,
    string Origem,
    string? UrlThumbnail,
    int? DuracaoMinutos,
    string Categoria,
    DateTime? PublicadoEm);

public sealed record DashboardFavoritoResponse(
    Guid ConteudoId,
    string Titulo,
    string Slug,
    string Tipo,
    string? UrlThumbnail,
    DateTime FavoritadoEm);

public sealed record DashboardProgressoConteudoResponse(
    Guid ConteudoId,
    string Titulo,
    string Slug,
    string Tipo,
    string Status,
    decimal Percentual,
    DateTime? UltimoAcessoEm);

public sealed record DashboardPlanoBiblicoResumoResponse(
    Guid Id,
    string Nome,
    int DuracaoDias,
    DateOnly DataInicio,
    DateOnly DataFimPrevista,
    int DiasConcluidos,
    int TotalDias,
    decimal Percentual);

public sealed record DashboardLeituraHojeResponse(
    Guid DiaId,
    int DiaNumero,
    int MesNumero,
    DateOnly? DataPrevista,
    string? LeiturasTexto,
    int? SalmoNumero,
    bool Concluido);

public sealed record DashboardEstatisticasResponse(
    int TotalFavoritos,
    int TotalConteudosConcluidos,
    decimal PercentualPlanoBiblico);
