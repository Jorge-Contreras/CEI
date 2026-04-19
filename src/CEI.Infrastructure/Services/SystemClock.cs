using CEI.Application.Common.Interfaces;

namespace CEI.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today(string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(UtcNow, timeZone);
        return DateOnly.FromDateTime(localTime);
    }
}
