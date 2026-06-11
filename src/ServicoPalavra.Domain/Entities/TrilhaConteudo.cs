namespace ServicoPalavra.Domain.Entities;

public sealed class TrilhaConteudo
{
    public Guid Id { get; set; }
    public Guid TrilhaFormacaoId { get; set; }
    public TrilhaFormacao TrilhaFormacao { get; set; } = null!;
    public Guid ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
    public int Ordem { get; set; }
    public bool Obrigatorio { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}
