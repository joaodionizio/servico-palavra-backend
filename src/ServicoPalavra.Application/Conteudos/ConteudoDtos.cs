using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.Conteudos;

public sealed record ConteudoResponse(Guid Id, string Titulo, string Slug, string? Descricao, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string Url, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, string Categoria, bool Publicado, bool Destaque, int? Ordem, DateTime? PublicadoEm);
public sealed record ConteudoRequest(string Titulo, string? Slug, string? Descricao, string? Resumo, TipoConteudo Tipo, OrigemConteudo Origem, string Url, string? UrlThumbnail, int? DuracaoMinutos, Guid CategoriaConteudoId, bool Publicado, bool Destaque, int? Ordem);
