namespace ServicoPalavra.Application.Categorias;

public sealed record CategoriaResponse(Guid Id, string Nome, string Slug, string? Descricao, string? Cor, string? Icone, int Ordem);
public sealed record CategoriaAdminResponse(Guid Id, string Nome, string Slug, string? Descricao, string? Cor, string? Icone, int Ordem, bool Ativo);
public sealed record CategoriaRequest(string Nome, string? Slug, string? Descricao, string? Cor, string? Icone, int Ordem, bool Ativo = true);
public sealed record CategoriaStatusRequest(bool Ativo);
