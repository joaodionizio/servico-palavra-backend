using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.PlanosBiblicos;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;
using ServicoPalavra.Infrastructure.Persistence;

namespace ServicoPalavra.IntegrationTests;

public sealed class AdaptiveCalendarEdgeCaseTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"servico-palavra-calendar-edge-{Guid.NewGuid():N}.db");

    [Fact]
    public async Task Future_duplicate_pending_dates_are_repaired_even_when_none_is_late()
    {
        await using var db = await CreateDbAsync();
        var today = new DateOnly(2026, 10, 29);
        var seeded = await SeedAsync(db, today, [
            new DateOnly(2026, 11, 1),
            new DateOnly(2026, 11, 1)
        ]);
        var clock = new FixedCalendarClock(today);
        var repository = new PlanoBiblicoRepository(db);
        var calendar = new PlanoBiblicoCalendarService(repository, db, clock);

        var changed = await calendar.RealignForReadAsync(seeded.UserId, seeded.PlanId);

        Assert.True(changed);
        db.ChangeTracker.Clear();
        var days = await db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == seeded.PlanId)
            .OrderBy(x => x.DiaNumero)
            .ToListAsync();

        Assert.Equal(today, days[0].DataPrevista);
        Assert.Equal(today.AddDays(1), days[1].DataPrevista);
        Assert.Equal(2, days.Select(x => x.DataPrevista).Distinct().Count());
    }

    [Fact]
    public async Task Dashboard_flags_stale_pending_day_even_when_completed_reading_exists_today()
    {
        await using var db = await CreateDbAsync();
        var today = new DateOnly(2026, 10, 29);
        var seeded = await SeedAsync(db, today, [
            today,
            today.AddDays(-1),
            today.AddDays(1)
        ], completedDayNumbers: [1]);
        var dashboard = new DashboardReadRepository(db);

        var before = await dashboard.GetMeAsync(seeded.UserId, today);

        Assert.True(before.NeedsCalendarRealignment);
        Assert.NotNull(before.Response.LeituraHoje);
        Assert.Equal(today, before.Response.LeituraHoje!.DataPrevista);
        Assert.True(before.Response.LeituraHoje.Concluido);

        var calendar = new PlanoBiblicoCalendarService(
            new PlanoBiblicoRepository(db),
            db,
            new FixedCalendarClock(today));
        Assert.True(await calendar.RealignForReadAsync(seeded.UserId, seeded.PlanId));

        db.ChangeTracker.Clear();
        var after = await dashboard.GetMeAsync(seeded.UserId, today);
        var days = await db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == seeded.PlanId)
            .OrderBy(x => x.DiaNumero)
            .ToListAsync();

        Assert.False(after.NeedsCalendarRealignment);
        Assert.Equal(today, days[0].DataPrevista);
        Assert.Equal(today.AddDays(1), days[1].DataPrevista);
        Assert.Equal(today.AddDays(2), days[2].DataPrevista);
    }

    public void Dispose()
    {
        TryDelete(_databasePath);
        TryDelete(_databasePath + "-shm");
        TryDelete(_databasePath + "-wal");
    }

    private async Task<AppDbContext> CreateDbAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static async Task<SeededPlan> SeedAsync(
        AppDbContext db,
        DateOnly today,
        IReadOnlyList<DateOnly> dates,
        IReadOnlyCollection<int>? completedDayNumbers = null)
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var now = new DateTime(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);
        var email = $"calendar-edge-{userId:N}@tests.local";

        db.Users.Add(new ApplicationUser
        {
            Id = userId,
            Nome = "Calendar Edge",
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            Ativo = true,
            CriadoEm = now
        });
        db.PlanosBiblicosUsuario.Add(new PlanoBiblicoUsuario
        {
            Id = planId,
            UsuarioId = userId,
            Nome = "Plano edge",
            DuracaoDias = dates.Count,
            DuracaoMeses = 1,
            DataInicio = today,
            DataFimPrevista = dates.Max(),
            Ativo = true,
            Status = StatusPlanoBiblico.Ativo,
            ModoCriacao = ModoCriacaoPlanoBiblico.NovoDoInicio,
            OrdemInicio = 1,
            OrdemFim = dates.Count,
            CriadoEm = now
        });

        var ids = new List<Guid>(dates.Count);
        for (var index = 0; index < dates.Count; index++)
        {
            var id = Guid.NewGuid();
            ids.Add(id);
            db.PlanosBiblicosDias.Add(new PlanoBiblicoDia
            {
                Id = id,
                PlanoBiblicoUsuarioId = planId,
                DiaNumero = index + 1,
                MesNumero = 1,
                DataPrevista = dates[index],
                LeiturasTexto = $"Leitura {index + 1}",
                CriadoEm = now
            });
        }

        foreach (var dayNumber in completedDayNumbers ?? [])
        {
            db.ProgressosLeitura.Add(new ProgressoLeitura
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                PlanoBiblicoDiaId = ids[dayNumber - 1],
                Concluido = true,
                ConcluidoEm = now,
                CriadoEm = now
            });
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return new SeededPlan(userId, planId);
    }

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed record SeededPlan(Guid UserId, Guid PlanId);

    private sealed class FixedCalendarClock(DateOnly today) : ICalendarClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 10, 29, 15, 0, 0, TimeSpan.Zero);
        public DateOnly Today { get; } = today;
    }
}
