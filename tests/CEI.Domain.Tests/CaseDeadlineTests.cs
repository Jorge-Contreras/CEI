using CEI.Domain.Cases;
using CEI.Domain.Enums;
using FluentAssertions;

namespace CEI.Domain.Tests;

public class CaseDeadlineTests
{
    [Fact]
    public void MarkCompleted_should_throw_when_deadline_was_cancelled()
    {
        var deadline = new CaseDeadline
        {
            DueDate = new DateOnly(2026, 4, 25),
            Description = "Presentar promoción"
        };

        deadline.MarkCancelled(Guid.NewGuid(), DateTime.UtcNow);

        var action = () => deadline.MarkCompleted(Guid.NewGuid(), DateTime.UtcNow);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RefreshOverdue_should_mark_pending_deadline_as_overdue_when_due_date_has_passed()
    {
        var deadline = new CaseDeadline
        {
            DueDate = new DateOnly(2026, 4, 10),
            Description = "Ofrecer pruebas"
        };

        deadline.RefreshOverdue(new DateOnly(2026, 4, 18));

        deadline.Status.Should().Be(DeadlineStatus.Overdue);
    }

    [Fact]
    public void MarkPending_should_clear_completion_and_cancellation_metadata()
    {
        var deadline = new CaseDeadline
        {
            DueDate = new DateOnly(2026, 4, 20),
            Description = "Acudir a audiencia"
        };

        deadline.MarkCompleted(Guid.NewGuid(), DateTime.UtcNow);
        deadline.MarkPending();

        deadline.Status.Should().Be(DeadlineStatus.Pending);
        deadline.CompletedOnUtc.Should().BeNull();
        deadline.CancelledOnUtc.Should().BeNull();
    }
}
