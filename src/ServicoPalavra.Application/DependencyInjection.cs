using Microsoft.Extensions.DependencyInjection;
using ServicoPalavra.Application.Auth;
using ServicoPalavra.Application.Categorias;
using ServicoPalavra.Application.Conteudos;
using ServicoPalavra.Application.Dashboard;
using ServicoPalavra.Application.Favoritos;
using ServicoPalavra.Application.PlanosBiblicos;
using ServicoPalavra.Application.Progresso;

namespace ServicoPalavra.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IConteudoService, ConteudoService>();
        services.AddScoped<IFavoritoService, FavoritoService>();
        services.AddScoped<IProgressoService, ProgressoService>();
        services.AddScoped<IBiblePlanGeneratorService, BiblePlanGeneratorService>();
        services.AddScoped<IPlanoBiblicoService, PlanoBiblicoService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
