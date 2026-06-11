namespace ServicoPalavra.Domain.Entities;

public sealed class PlanoBiblicoDia
{
    public Guid Id { get; set; }
    public Guid PlanoBiblicoUsuarioId { get; set; }
    public PlanoBiblicoUsuario PlanoBiblicoUsuario { get; set; } = null!;
    public int DiaNumero { get; set; }
    public int MesNumero { get; set; }
    public DateOnly? DataPrevista { get; set; }
    public string? LeiturasTexto { get; set; }
    public int? SalmoNumero { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<PlanoBiblicoDiaCapitulo> Capitulos { get; set; } = new List<PlanoBiblicoDiaCapitulo>();
}
