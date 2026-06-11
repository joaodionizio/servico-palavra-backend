namespace ServicoPalavra.Application.Auth;

public sealed record RegisterRequest(string Nome, string Email, string Senha);
public sealed record LoginRequest(string Email, string Senha);
public sealed record AuthResponse(UsuarioResponse Usuario);
public sealed record UsuarioResponse(Guid Id, string Nome, string Email, IReadOnlyCollection<string> Roles);
