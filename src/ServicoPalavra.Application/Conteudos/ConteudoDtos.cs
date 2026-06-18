using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.Conteudos;

public sealed record ConteudoResponse(Guid Id, string Titulo, string Slug, string? Descricao, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string Url, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, string Categoria, bool Publicado, bool Destaque, int? Ordem, DateTime? PublicadoEm);
public sealed record ConteudoRequest(string Titulo, string? Slug, string? Descricao, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string Url, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, bool Publicado, bool Destaque, int? Ordem, IReadOnlyList<MaterialApoioRequest>? MateriaisApoio = null);
public sealed record ConteudoListQuery(string? Busca, string? CategoriaSlug, TipoConteudo? Tipo, int Pagina = 1, int TamanhoPagina = 12);
public sealed record ConteudoAdminListQuery(string? Busca, string? CategoriaSlug, TipoConteudo? Tipo, bool? Publicado, int Pagina = 1, int TamanhoPagina = 20);
public sealed record ConteudoPublicacaoRequest(bool Publicado);
public sealed record PagedResponse<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int TotalItens, int TotalPaginas);
public sealed record ConteudoResumoResponse(Guid Id, string Titulo, string Slug, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, string Categoria, bool Destaque, DateTime? PublicadoEm);
public sealed record ConteudoDetalheResponse(Guid Id, string Titulo, string Slug, string? Descricao, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string Url, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, string Categoria, bool Publicado, bool Destaque, int? Ordem, DateTime? PublicadoEm, IReadOnlyList<MaterialApoioResponse> MateriaisApoio, bool Favorito, bool Concluido);
public sealed record MaterialApoioResponse(Guid Id, string Titulo, string? Descricao, TipoMaterialApoio Tipo, string Url, int Ordem);
public sealed record MaterialApoioRequest(string Titulo, string? Descricao, TipoMaterialApoio Tipo, string Url, int Ordem, bool Ativo = true);
public sealed record MaterialApoioAdminResponse(Guid Id, string Titulo, string? Descricao, TipoMaterialApoio Tipo, string Url, int Ordem, bool Ativo);
public sealed record ConteudoAdminResumoResponse(Guid Id, string Titulo, string Slug, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, string Categoria, bool Publicado, bool Destaque, int? Ordem, DateTime? PublicadoEm);
public sealed record ConteudoAdminDetalheResponse(Guid Id, string Titulo, string Slug, string? Descricao, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string Url, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, string Categoria, bool Publicado, bool Destaque, int? Ordem, DateTime? PublicadoEm, IReadOnlyList<MaterialApoioAdminResponse> MateriaisApoio);
