using System.Security.Claims;
using ServicoPalavra.Application.Abstractions;

namespace ServicoPalavra.Api.Services;

public sealed class CurrentUser : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    public string? Perfil => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
    public IReadOnlyCollection<string> Roles => _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray() ?? [];
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
    public bool IsAdmin => _httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true;
}
