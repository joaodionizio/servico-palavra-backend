using System.Text;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ServicoPalavra.Api.Middleware;
using ServicoPalavra.Api.Services;
using ServicoPalavra.Application;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Infrastructure;
using ServicoPalavra.Infrastructure.Persistence;
using ServicoPalavra.Infrastructure.Persistence.Seed;

var startupDiagnostics = string.Equals(Environment.GetEnvironmentVariable("STARTUP_DIAGNOSTICS"), "true", StringComparison.OrdinalIgnoreCase);
void StartupTrace(string message)
{
    if (startupDiagnostics)
    {
        Console.Error.WriteLine($"[startup] {message}");
    }
}

StartupTrace("Main iniciado.");
var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
});
builder.WebHost.UseKestrel();
builder.Logging.AddConsole();
StartupTrace("Builder criado.");
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
var configuredUrls = builder.Configuration["ASPNETCORE_URLS"] ?? builder.Configuration["urls"];
if (!string.IsNullOrWhiteSpace(configuredUrls))
{
    builder.WebHost.UseUrls(configuredUrls);
}

StartupTrace("Configuracao carregada.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Servico da Palavra API", Version = "v2" });
    options.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
    {
        Name = "__Host-ServicoPalavra",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Cookie"
                }
            },
            []
        }
    });
});
StartupTrace("Swagger configurado.");

builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        var origins = (builder.Configuration["ALLOWED_ORIGINS"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"]);

        policy
            .WithOrigins(origins)
            .WithHeaders("Content-Type", "X-Correlation-ID", "X-CSRF-TOKEN")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS");

        if (origins.Length > 0)
        {
            policy.AllowCredentials();
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    var isTesting = builder.Environment.IsEnvironment("Testing");
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = isTesting ? 1000 : 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("sensitive", limiter =>
    {
        limiter.PermitLimit = isTesting ? 1000 : 20;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("general", limiter =>
    {
        limiter.PermitLimit = isTesting ? 1000 : 120;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "authenticated"
            : context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = isTesting ? 1000 : 180,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ICurrentUserService>(sp => (CurrentUser)sp.GetRequiredService<ICurrentUser>());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
StartupTrace("Servicos de aplicacao e infraestrutura configurados.");
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "__Host-ServicoPalavra";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing")
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-ServicoPalavra-Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing")
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();
StartupTrace("Identity, cookies, antiforgery e autorizacao configurados.");

var app = builder.Build();
StartupTrace("Aplicacao construida.");

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var isEfTool = Assembly.GetEntryAssembly()?.GetName().Name == "ef";
StartupTrace("Aplicacao configurada; iniciando bootstrap de banco.");
if (!EF.IsDesignTime && !isEfTool)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var databaseProvider = (app.Configuration["DATABASE_PROVIDER"] ?? "sqlite").Trim().ToLowerInvariant();
    if (app.Environment.IsEnvironment("Testing") || databaseProvider == "sqlite")
    {
        StartupTrace("Executando EnsureCreated para SQLite/Testing.");
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        StartupTrace("Executando Migrate para provider relacional configurado.");
        await db.Database.MigrateAsync();
    }

    StartupTrace("Executando seed de roles/admin.");
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    StartupTrace("Bootstrap de banco concluido.");
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
app.UseCors("DefaultCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method)
        || HttpMethods.IsPut(context.Request.Method)
        || HttpMethods.IsPatch(context.Request.Method)
        || HttpMethods.IsDelete(context.Request.Method))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        await antiforgery.ValidateRequestAsync(context);
    }

    await next();
});

app.MapHealthChecks("/health").RequireRateLimiting("general");
app.MapControllers().RequireRateLimiting("general");

if (!EF.IsDesignTime && !isEfTool)
{
    StartupTrace("Chamando app.Run().");
    app.Run();
}

public partial class Program;
