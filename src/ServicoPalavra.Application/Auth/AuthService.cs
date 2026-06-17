using Microsoft.AspNetCore.Identity;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (request.Senha.Length is < 6 or > 8)
        {
            throw new AppException("Nao foi possivel concluir o cadastro.");
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new AppException("Nao foi possivel concluir o cadastro.");
        }

        var usuario = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(usuario, request.Senha);
        if (!result.Succeeded)
        {
            throw new AppException("Nao foi possivel concluir o cadastro.");
        }

        await _userManager.AddToRoleAsync(usuario, "Usuario");
        await _signInManager.SignInAsync(usuario, isPersistent: false);

        return new AuthResponse(await ToResponseAsync(usuario));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await _userManager.FindByEmailAsync(email);
        if (usuario is null || !usuario.Ativo)
        {
            throw new AppException("Credenciais invalidas.", 401);
        }

        var result = await _signInManager.PasswordSignInAsync(usuario, request.Senha, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new AppException("Credenciais invalidas.", 401);
        }

        usuario.UltimoAcessoEm = DateTime.UtcNow;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await _userManager.UpdateAsync(usuario);

        return new AuthResponse(await ToResponseAsync(usuario));
    }

    public Task LogoutAsync() => _signInManager.SignOutAsync();

    public async Task<UsuarioResponse> MeAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString())
            ?? throw new AppException("Usuario nao encontrado.", 404);

        return await ToResponseAsync(usuario);
    }

    private async Task<UsuarioResponse> ToResponseAsync(ApplicationUser usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email ?? string.Empty, roles.ToArray());
    }
}
