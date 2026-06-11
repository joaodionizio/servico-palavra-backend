namespace ServicoPalavra.Domain.Entities;

public sealed class TrilhaFormacao
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Resumo { get; set; }
    public string? ImagemUrl { get; set; }
    public bool Publicado { get; set; }
    public bool Destaque { get; set; }
    public int? Ordem { get; set; }
    public Guid CriadoPorUsuarioId { get; set; }
    public ApplicationUser CriadoPorUsuario { get; set; } = null!;
    public DateTime? PublicadoEm { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<TrilhaConteudo> Conteudos { get; set; } = new List<TrilhaConteudo>();
}
