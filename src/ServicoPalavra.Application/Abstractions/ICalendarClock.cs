namespace ServicoPalavra.Application.Abstractions;

public interface ICalendarClock
{
    DateTimeOffset UtcNow { get; }
    DateOnly Today { get; }
}
