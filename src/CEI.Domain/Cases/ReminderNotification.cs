using CEI.Domain.Common;
using CEI.Domain.Enums;

namespace CEI.Domain.Cases;

public class ReminderNotification : AuditableEntity
{
    public Guid CaseDeadlineId { get; set; }

    public Guid UserId { get; set; }

    public ReminderKind Kind { get; set; }

    public DateOnly TriggerDate { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; private set; }

    public DateTime? ReadOnUtc { get; private set; }

    public bool IsCleared { get; private set; }

    public DateTime? ClearedOnUtc { get; private set; }

    public CaseDeadline CaseDeadline { get; set; } = null!;

    public void MarkRead(DateTime readOnUtc)
    {
        IsRead = true;
        ReadOnUtc = readOnUtc;
    }

    public void MarkUnread()
    {
        IsRead = false;
        ReadOnUtc = null;
    }

    public void Clear(DateTime clearedOnUtc)
    {
        IsCleared = true;
        ClearedOnUtc = clearedOnUtc;
    }
}
