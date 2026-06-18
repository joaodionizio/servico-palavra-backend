using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Domain.Entities;

public sealed class Conteudo
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Resumo { get; set; }
    public TipoConteudo Tipo { get; set; }
    public OrigemConteudo Origem { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? UrlThumbnail { get; set; }
    public int? DuracaoMinutos { get; set; }
    public Guid? CategoriaConteudoId { get; set; }
    public CategoriaConteudo? CategoriaConteudo { get; set; }
    public Guid CriadoPorUsuarioId { get; set; }
    public ApplicationUser CriadoPorUsuario { get; set; } = null!;
    public bool Publicado { get; set; }
    public bool Destaque { get; set; }
    public int? Ordem { get; set; }
    public DateTime? PublicadoEm { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<MaterialApoio> MateriaisApoio { get; set; } = new List<MaterialApoio>();
}
