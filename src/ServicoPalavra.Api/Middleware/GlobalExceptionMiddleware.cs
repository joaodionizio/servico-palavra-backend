using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using ServicoPalavra.Application.Common;

namespace ServicoPalavra.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(ex.Message));
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Token CSRF invalido."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro nao tratado.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = _environment.IsEnvironment("Testing") ? ex.Message : "Erro interno no servidor.";
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message));
        }
    }
}
