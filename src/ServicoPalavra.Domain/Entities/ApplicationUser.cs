using Microsoft.AspNetCore.Identity;

namespace ServicoPalavra.Domain.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime? UltimoAcessoEm { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Conteudo> ConteudosCriados { get; set; } = new List<Conteudo>();
}
