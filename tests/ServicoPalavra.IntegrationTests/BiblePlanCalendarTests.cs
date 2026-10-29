using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.PlanosBiblicos;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;
using ServicoPalavra.Infrastructure.Persistence;
using ServicoPalavra.Infrastructure.Time;

namespace ServicoPalavra.IntegrationTests;

public sealed class BiblePlanCalendarTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"servico-palavra-calendar-{Guid.NewGuid():N}.db");

    [Fact]
    public void CalendarClock_uses_local_calendar_date_before_sao_paulo_midnight()
    {
        var utcNow = new DateTimeOffset(2026, 10, 30, 0, 30, 0, TimeSpan.Zero);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Calendar:TimeZone"] = "America/Sao_Paulo"
            })
            .Build();
        var clock = new CalendarClock(new FixedTimeProvider(utcNow), configuration);

        Assert.Equal(utcNow, clock.UtcNow);
        Assert.Equal(new DateOnly(2026, 10, 29), clock.Today);
    }

    [Fact]
    public async Task RealignForRead_moves_only_pending_days_and_is_idempotent()
    {
        await using var db = await CreateDbAsync();
        var seeded = await SeedPlanAsync(
            db,
            [new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 21), new DateOnly(2026, 10, 22)],
            completedDayNumbers: [1]);
        var clock = new FixedCalendarClock(
            new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 10, 29));
        var service = CreateCalendarService(db, clock);

        var changed = await service.RealignForReadAsync(seeded.UserId, seeded.PlanId);
        Assert.True(changed);

        db.ChangeTracker.Clear();
        var days = await db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == seeded.PlanId)
            .OrderBy(x => x.DiaNumero)
            .ToListAsync();
        var plan = await db.PlanosBiblicosUsuario.AsNoTracking().SingleAsync(x => x.Id == seeded.PlanId);

        Assert.Equal(new DateOnly(2026, 10, 20), days[0].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 29), days[1].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 30), days[2].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 30), plan.DataFimPrevista);
        Assert.Equal(days.Count, days.Where(x => x.DataPrevista.HasValue).Select(x => x.DataPrevista).Distinct().Count());

        var changedAgain = await service.RealignForReadAsync(seeded.UserId, seeded.PlanId);
        Assert.False(changedAgain);
    }

    [Fact]
    public async Task Completing_day_records_utc_timestamp_and_moves_pending_from_local_tomorrow()
    {
        await using var db = await CreateDbAsync();
        var seeded = await SeedPlanAsync(
            db,
            [new DateOnly(2026, 10, 29), new DateOnly(2026, 10, 29), new DateOnly(2026, 10, 30)]);
        var utcNow = new DateTimeOffset(2026, 10, 29, 23, 30, 0, TimeSpan.Zero);
        var clock = new FixedCalendarClock(utcNow, new DateOnly(2026, 10, 29));
        var repository = new PlanoBiblicoRepository(db);
        var calendar = new PlanoBiblicoCalendarService(repository, db, clock);
        var service = new PlanoBiblicoService(
            repository,
            new BiblePlanGeneratorService(repository),
            calendar,
            clock,
            db);

        await service.ConcluirDiaAsync(seeded.UserId, seeded.DayIds[0]);

        db.ChangeTracker.Clear();
        var progress = await db.ProgressosLeitura.AsNoTracking().SingleAsync(x => x.PlanoBiblicoDiaId == seeded.DayIds[0]);
        var days = await db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == seeded.PlanId)
            .OrderBy(x => x.DiaNumero)
            .ToListAsync();

        Assert.True(progress.Concluido);
        Assert.Equal(utcNow.UtcDateTime, progress.ConcluidoEm);
        Assert.Equal(new DateOnly(2026, 10, 29), days[0].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 30), days[1].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 31), days[2].DataPrevista);
    }

    [Fact]
    public async Task Reopening_day_reenters_pending_sequence_from_local_today_without_touching_other_history()
    {
        await using var db = await CreateDbAsync();
        var seeded = await SeedPlanAsync(
            db,
            [new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 29), new DateOnly(2026, 10, 30)],
            completedDayNumbers: [1, 2]);
        var clock = new FixedCalendarClock(
            new DateTimeOffset(2026, 10, 29, 18, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 10, 29));
        var repository = new PlanoBiblicoRepository(db);
        var calendar = new PlanoBiblicoCalendarService(repository, db, clock);
        var service = new PlanoBiblicoService(
            repository,
            new BiblePlanGeneratorService(repository),
            calendar,
            clock,
            db);

        await service.DesmarcarDiaAsync(seeded.UserId, seeded.DayIds[1]);

        db.ChangeTracker.Clear();
        var days = await db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == seeded.PlanId)
            .OrderBy(x => x.DiaNumero)
            .ToListAsync();
        var reopened = await db.ProgressosLeitura.AsNoTracking().SingleAsync(x => x.PlanoBiblicoDiaId == seeded.DayIds[1]);

        Assert.Equal(new DateOnly(2026, 10, 20), days[0].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 29), days[1].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 30), days[2].DataPrevista);
        Assert.False(reopened.Concluido);
        Assert.Null(reopened.ConcluidoEm);
    }

    [Fact]
    public async Task Realignment_skips_dates_occupied_by_completed_history()
    {
        await using var db = await CreateDbAsync();
        var seeded = await SeedPlanAsync(
            db,
            [new DateOnly(2026, 10, 30), new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 21)],
            completedDayNumbers: [1]);
        var clock = new FixedCalendarClock(
            new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 10, 29));
        var service = CreateCalendarService(db, clock);

        await service.RealignForReadAsync(seeded.UserId, seeded.PlanId);

        db.ChangeTracker.Clear();
        var days = await db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == seeded.PlanId)
            .OrderBy(x => x.DiaNumero)
            .ToListAsync();

        Assert.Equal(new DateOnly(2026, 10, 30), days[0].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 29), days[1].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 31), days[2].DataPrevista);
        Assert.Equal(3, days.Select(x => x.DataPrevista).Distinct().Count());
    }

    [Fact]
    public async Task Fully_completed_plan_keeps_history_and_reconciles_end_date()
    {
        await using var db = await CreateDbAsync();
        var seeded = await SeedPlanAsync(
            db,
            [new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 21)],
            completedDayNumbers: [1, 2],
            dataFimOverride: new DateOnly(2026, 12, 31));
        var clock = new FixedCalendarClock(
            new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero),
            new DateOnly(2026, 10, 29));
        var service = CreateCalendarService(db, clock);

        var changed = await service.RealignForReadAsync(seeded.UserId, seeded.PlanId);
        Assert.True(changed);

        db.ChangeTracker.Clear();
        var plan = await db.PlanosBiblicosUsuario.AsNoTracking().SingleAsync(x => x.Id == seeded.PlanId);
        var days = await db.PlanosBiblicosDias
            .AsNoTracking()
            .Where(x => x.PlanoBiblicoUsuarioId == seeded.PlanId)
            .OrderBy(x => x.DiaNumero)
            .ToListAsync();

        Assert.Equal(new DateOnly(2026, 10, 21), plan.DataFimPrevista);
        Assert.Equal(new DateOnly(2026, 10, 20), days[0].DataPrevista);
        Assert.Equal(new DateOnly(2026, 10, 21), days[1].DataPrevista);
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

    private static PlanoBiblicoCalendarService CreateCalendarService(AppDbContext db, ICalendarClock clock)
    {
        var repository = new PlanoBiblicoRepository(db);
        return new PlanoBiblicoCalendarService(repository, db, clock);
    }

    private static async Task<SeededPlan> SeedPlanAsync(
        AppDbContext db,
        IReadOnlyList<DateOnly> dates,
        IReadOnlyCollection<int>? completedDayNumbers = null,
        DateOnly? dataFimOverride = null)
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var now = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);
        var email = $"calendar-{userId:N}@tests.local";
        var user = new ApplicationUser
        {
            Id = userId,
            Nome = "Calendar Test",
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            Ativo = true,
            CriadoEm = now
        };
        var plan = new PlanoBiblicoUsuario
        {
            Id = planId,
            UsuarioId = userId,
            Nome = "Plano adaptativo",
            DuracaoDias = dates.Count,
            DuracaoMeses = 1,
            DuracaoAnos = 0,
            DataInicio = dates[0],
            DataFimPrevista = dataFimOverride ?? dates.Max(),
            Ativo = true,
            Status = StatusPlanoBiblico.Ativo,
            ModoCriacao = ModoCriacaoPlanoBiblico.NovoDoInicio,
            OrdemInicio = 1,
            OrdemFim = dates.Count,
            CriadoEm = now
        };
        var dayIds = new List<Guid>(dates.Count);
        var days = dates.Select((date, index) =>
        {
            var id = Guid.NewGuid();
            dayIds.Add(id);
            return new PlanoBiblicoDia
            {
                Id = id,
                PlanoBiblicoUsuarioId = planId,
                DiaNumero = index + 1,
                MesNumero = 1,
                DataPrevista = date,
                LeiturasTexto = $"Leitura {index + 1}",
                CriadoEm = now
            };
        }).ToList();

        db.Users.Add(user);
        db.PlanosBiblicosUsuario.Add(plan);
        db.PlanosBiblicosDias.AddRange(days);

        foreach (var dayNumber in completedDayNumbers ?? [])
        {
            db.ProgressosLeitura.Add(new ProgressoLeitura
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                PlanoBiblicoDiaId = dayIds[dayNumber - 1],
                Concluido = true,
                ConcluidoEm = now,
                CriadoEm = now
            });
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return new SeededPlan(userId, planId, dayIds);
    }

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed record SeededPlan(Guid UserId, Guid PlanId, IReadOnlyList<Guid> DayIds);

    private sealed class FixedCalendarClock(DateTimeOffset utcNow, DateOnly today) : ICalendarClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
        public DateOnly Today { get; } = today;
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
