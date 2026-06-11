namespace ServicoPalavra.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync();
    Task<UsuarioResponse> MeAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
