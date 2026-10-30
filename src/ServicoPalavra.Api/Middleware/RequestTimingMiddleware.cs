using System.Diagnostics;

namespace ServicoPalavra.Api.Middleware;

public sealed class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;
    private readonly double _slowRequestThresholdMs;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _slowRequestThresholdMs = Math.Max(
            1,
            configuration.GetValue<double?>("Observability:SlowRequestThresholdMs") ?? 1000);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Server-Timing"] = $"app;dur={stopwatch.Elapsed.TotalMilliseconds:0.0}";
            return Task.CompletedTask;
        });

        await _next(context);
        stopwatch.Stop();

        if (stopwatch.Elapsed.TotalMilliseconds >= _slowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow HTTP request {Method} {Path} returned {StatusCode} in {ElapsedMs:0.0} ms",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
