using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;
    private readonly IAntiforgery _antiforgery;

    public AuthController(IAuthService authService, ICurrentUser currentUser, IAntiforgery antiforgery)
    {
        _authService = authService;
        _currentUser = currentUser;
        _antiforgery = antiforgery;
    }

    [HttpGet("csrf")]
    public IActionResult Csrf()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return OkResponse(new { token = tokens.RequestToken });
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _authService.RegisterAsync(request, cancellationToken));

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _authService.LoginAsync(request, cancellationToken));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return OkResponse("Logout realizado.");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken) =>
        OkResponse(await _authService.MeAsync(CurrentUserId(_currentUser), cancellationToken));

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateMeRequest request, CancellationToken cancellationToken) =>
        OkResponse(await _authService.UpdateMeAsync(CurrentUserId(_currentUser), request, cancellationToken));
}
