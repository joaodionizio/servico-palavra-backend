namespace ServicoPalavra.Domain.Entities;

public sealed class Favorito
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public Guid ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
    public DateTime CriadoEm { get; set; }
}
