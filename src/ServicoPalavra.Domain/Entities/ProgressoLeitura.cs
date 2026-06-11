namespace ServicoPalavra.Domain.Entities;

public sealed class ProgressoLeitura
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public Guid PlanoBiblicoDiaId { get; set; }
    public PlanoBiblicoDia PlanoBiblicoDia { get; set; } = null!;
    public bool Concluido { get; set; }
    public DateTime? ConcluidoEm { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
