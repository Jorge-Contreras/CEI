namespace CEI.Application.Common.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }

    DateOnly Today(string timeZoneId);
}
