namespace CEI.Domain.Common;

public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedOnUtc { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? ModifiedByUserId { get; set; }
}
