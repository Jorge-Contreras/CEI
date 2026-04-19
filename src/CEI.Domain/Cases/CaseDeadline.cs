using CEI.Domain.Common;
using CEI.Domain.Enums;

namespace CEI.Domain.Cases;

public class CaseDeadline : AuditableEntity
{
    public Guid LegalCaseId { get; set; }

    public Guid? RelatedCaseEventId { get; set; }

    public Guid ResponsibleUserId { get; set; }

    public DateOnly DueDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public DeadlineStatus Status { get; private set; } = DeadlineStatus.Pending;

    public DateTime? CompletedOnUtc { get; private set; }

    public Guid? CompletedByUserId { get; private set; }

    public DateTime? CancelledOnUtc { get; private set; }

    public Guid? CancelledByUserId { get; private set; }

    public LegalCase LegalCase { get; set; } = null!;

    public CaseEvent? RelatedCaseEvent { get; set; }

    public ICollection<ReminderNotification> Reminders { get; set; } = new List<ReminderNotification>();

    public void MarkCompleted(Guid userId, DateTime completedOnUtc)
    {
        if (Status == DeadlineStatus.Cancelled)
        {
            throw new InvalidOperationException("A cancelled deadline cannot be completed.");
        }

        Status = DeadlineStatus.Completed;
        CompletedByUserId = userId;
        CompletedOnUtc = completedOnUtc;
        CancelledByUserId = null;
        CancelledOnUtc = null;
    }

    public void MarkCancelled(Guid userId, DateTime cancelledOnUtc)
    {
        if (Status == DeadlineStatus.Completed)
        {
            throw new InvalidOperationException("A completed deadline cannot be cancelled.");
        }

        Status = DeadlineStatus.Cancelled;
        CancelledByUserId = userId;
        CancelledOnUtc = cancelledOnUtc;
    }

    public void MarkPending()
    {
        Status = DeadlineStatus.Pending;
        CompletedByUserId = null;
        CompletedOnUtc = null;
        CancelledByUserId = null;
        CancelledOnUtc = null;
    }

    public void RefreshOverdue(DateOnly today)
    {
        if (Status is DeadlineStatus.Completed or DeadlineStatus.Cancelled)
        {
            return;
        }

        Status = DueDate < today ? DeadlineStatus.Overdue : DeadlineStatus.Pending;
    }
}
