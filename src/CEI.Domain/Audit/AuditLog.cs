using CEI.Domain.Common;
using CEI.Domain.Enums;

namespace CEI.Domain.Audit;

public class AuditLog : Entity
{
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public AuditOperation Operation { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public bool IsSuccessful { get; set; }

    public string? MetadataJson { get; set; }

    public string? RemoteIpAddress { get; set; }
}
