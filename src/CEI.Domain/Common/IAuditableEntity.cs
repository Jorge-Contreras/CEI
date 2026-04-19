namespace CEI.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedOnUtc { get; set; }

    DateTime? ModifiedOnUtc { get; set; }

    string? CreatedByUserId { get; set; }

    string? ModifiedByUserId { get; set; }
}
