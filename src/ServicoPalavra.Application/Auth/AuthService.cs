using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPasswordResetEmailSender _passwordResetEmailSender;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPasswordResetEmailSender passwordResetEmailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _passwordResetEmailSender = passwordResetEmailSender;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(request.Senha))
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

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(email))
        {
            throw new AppException("Email invalido.");
        }

        var usuario = await _userManager.FindByEmailAsync(email);
        if (usuario is null || !usuario.Ativo)
        {
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var encodedToken = EncodeToken(token);

        await _passwordResetEmailSender.SendPasswordResetAsync(
            usuario.Email ?? email,
            usuario.Nome,
            usuario.Id,
            encodedToken,
            cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NovaSenha))
        {
            throw new AppException("Nao foi possivel redefinir a senha.");
        }

        var usuario = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (usuario is null || !usuario.Ativo)
        {
            throw new AppException("Nao foi possivel redefinir a senha.");
        }

        string token;
        try
        {
            token = DecodeToken(request.Token);
        }
        catch (FormatException)
        {
            throw new AppException("Link de recuperacao invalido ou expirado.");
        }

        var result = await _userManager.ResetPasswordAsync(usuario, token, request.NovaSenha);
        if (!result.Succeeded)
        {
            throw new AppException("Link de recuperacao invalido ou expirado.");
        }
    }

    public Task LogoutAsync() => _signInManager.SignOutAsync();

    public async Task<UsuarioResponse> MeAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString())
            ?? throw new AppException("Usuario nao encontrado.", 404);

        return await ToResponseAsync(usuario);
    }

    public async Task<UsuarioResponse> UpdateMeAsync(Guid usuarioId, UpdateMeRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString())
            ?? throw new AppException("Usuario nao encontrado.", 404);

        var nome = request.Nome.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new AppException("Nome e obrigatorio.");
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            throw new AppException("Email invalido.");
        }

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null && existing.Id != usuario.Id)
        {
            throw new AppException("Email ja esta em uso.");
        }

        usuario.Nome = nome;
        usuario.AtualizadoEm = DateTime.UtcNow;

        if (!string.Equals(usuario.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await _userManager.SetEmailAsync(usuario, email);
            if (!emailResult.Succeeded)
            {
                throw new AppException("Nao foi possivel atualizar o email.");
            }

            var userNameResult = await _userManager.SetUserNameAsync(usuario, email);
            if (!userNameResult.Succeeded)
            {
                throw new AppException("Nao foi possivel atualizar o email.");
            }

            usuario.EmailConfirmed = false;
        }

        var result = await _userManager.UpdateAsync(usuario);
        if (!result.Succeeded)
        {
            throw new AppException("Nao foi possivel atualizar o perfil.");
        }

        await _signInManager.RefreshSignInAsync(usuario);
        return await ToResponseAsync(usuario);
    }

    private async Task<UsuarioResponse> ToResponseAsync(ApplicationUser usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email ?? string.Empty, roles.ToArray());
    }

    private static string EncodeToken(string token) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string DecodeToken(string encodedToken)
    {
        var normalized = encodedToken.Replace('-', '+').Replace('_', '/');
        normalized += normalized.Length % 4 switch
        {
            0 => string.Empty,
            2 => "==",
            3 => "=",
            _ => throw new FormatException("Token Base64Url invalido.")
        };

        return Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
    }
}
