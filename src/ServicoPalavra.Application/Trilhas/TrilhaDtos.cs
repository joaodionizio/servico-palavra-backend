namespace ServicoPalavra.Application.Trilhas;

public sealed record TrilhaResponse(Guid Id, string Titulo, string Slug, string? Descricao, string? Resumo, string? ImagemUrl, bool Publicado, bool Destaque, int? Ordem, IReadOnlyList<TrilhaConteudoResponse> Conteudos);
public sealed record TrilhaConteudoResponse(Guid ConteudoId, string Titulo, int Ordem, bool Obrigatorio);
public sealed record TrilhaRequest(string Titulo, string? Slug, string? Descricao, string? Resumo, string? ImagemUrl, bool Publicado, bool Destaque, int? Ordem);
public sealed record AddTrilhaConteudoRequest(Guid ConteudoId, int Ordem, bool Obrigatorio = true);
public sealed record ReordenarTrilhaConteudoRequest(IReadOnlyList<TrilhaConteudoOrdemRequest> Conteudos);
public sealed record TrilhaConteudoOrdemRequest(Guid ConteudoId, int Ordem);
