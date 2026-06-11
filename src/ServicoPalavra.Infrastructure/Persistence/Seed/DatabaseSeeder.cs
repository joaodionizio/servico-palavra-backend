using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public DatabaseSeeder(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { "Usuario", "Admin", "Coordenador" })
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var email = _configuration["INITIAL_ADMIN_EMAIL"];
        var password = _configuration["INITIAL_ADMIN_PASSWORD"];
        var name = _configuration["INITIAL_ADMIN_NAME"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        email = email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Nome = name.Trim(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
