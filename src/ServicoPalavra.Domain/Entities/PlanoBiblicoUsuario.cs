using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Domain.Entities;

public sealed class PlanoBiblicoUsuario
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public string Nome { get; set; } = string.Empty;
    public int DuracaoDias { get; set; }
    public int DuracaoMeses { get; set; }
    public int DuracaoAnos { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFimPrevista { get; set; }
    public bool Ativo { get; set; } = true;
    public StatusPlanoBiblico Status { get; set; }
    public Guid? PlanoOrigemId { get; set; }
    public PlanoBiblicoUsuario? PlanoOrigem { get; set; }
    public ModoCriacaoPlanoBiblico ModoCriacao { get; set; }
    public int OrdemInicio { get; set; }
    public int? OrdemFim { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<PlanoBiblicoDia> Dias { get; set; } = new List<PlanoBiblicoDia>();
}
