namespace ServicoPalavra.Domain.Entities;

public sealed class PosicaoBiblicaUsuario
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public Guid? UltimaBaseBiblicaConcluidaId { get; set; }
    public BaseBiblica? UltimaBaseBiblicaConcluida { get; set; }
    public int UltimaOrdemConcluida { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
