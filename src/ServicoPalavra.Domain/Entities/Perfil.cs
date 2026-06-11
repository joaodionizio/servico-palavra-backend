namespace ServicoPalavra.Domain.Entities;

public sealed class Perfil
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<ApplicationUser> Usuarios { get; set; } = new List<ApplicationUser>();
}
