using Microsoft.Extensions.Configuration;
using ServicoPalavra.Application.Abstractions;

namespace ServicoPalavra.Infrastructure.Time;

public sealed class CalendarClock : ICalendarClock
{
    public const string DefaultTimeZoneId = "America/Sao_Paulo";

    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _timeZone;

    public CalendarClock(TimeProvider timeProvider, IConfiguration configuration)
    {
        _timeProvider = timeProvider;
        var configuredTimeZone = configuration["Calendar:TimeZone"]?.Trim();
        var timeZoneId = string.IsNullOrWhiteSpace(configuredTimeZone) ? DefaultTimeZoneId : configuredTimeZone;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();

    public DateOnly Today
    {
        get
        {
            var localNow = TimeZoneInfo.ConvertTime(UtcNow, _timeZone);
            return DateOnly.FromDateTime(localNow.DateTime);
        }
    }
}
