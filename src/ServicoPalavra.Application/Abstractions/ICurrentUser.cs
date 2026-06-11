namespace ServicoPalavra.Application.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Perfil { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}

public interface ICurrentUserService : ICurrentUser;
