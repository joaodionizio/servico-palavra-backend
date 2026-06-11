using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Domain.Entities;

public sealed class ProgressoConteudo
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public Guid ConteudoId { get; set; }
    public Conteudo Conteudo { get; set; } = null!;
    public StatusProgressoConteudo Status { get; set; }
    public decimal Percentual { get; set; }
    public DateTime? IniciadoEm { get; set; }
    public DateTime? ConcluidoEm { get; set; }
    public DateTime? UltimoAcessoEm { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
