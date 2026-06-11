using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Domain.Entities;

public sealed class MaterialApoio
{
    public Guid Id { get; set; }
    public Guid ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoMaterialApoio Tipo { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
