namespace ServicoPalavra.Domain.Entities;

public sealed class BaseBiblica
{
    public Guid Id { get; set; }
    public int Ordem { get; set; }
    public string Livro { get; set; } = string.Empty;
    public int Capitulo { get; set; }
    public string Grupo { get; set; } = string.Empty;
    public string? Subgrupo { get; set; }
    public string Testamento { get; set; } = string.Empty;
    public int? TempoEstimadoMinutos { get; set; }
    public int QuantidadeVersiculos { get; set; }
    public int PesoLeitura { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
