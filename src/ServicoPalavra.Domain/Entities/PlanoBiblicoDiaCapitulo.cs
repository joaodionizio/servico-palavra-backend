namespace ServicoPalavra.Domain.Entities;

public sealed class PlanoBiblicoDiaCapitulo
{
    public Guid Id { get; set; }
    public Guid PlanoBiblicoDiaId { get; set; }
    public PlanoBiblicoDia PlanoBiblicoDia { get; set; } = null!;
    public Guid BaseBiblicaId { get; set; }
    public BaseBiblica BaseBiblica { get; set; } = null!;
    public int Ordem { get; set; }
    public DateTime CriadoEm { get; set; }
}
