using CEI.Domain.Enums;

namespace CEI.Application.Common.Interfaces;

public interface IAuditWriter
{
    Task WriteAsync(AuditWriteRequest request, CancellationToken cancellationToken = default);
}

public sealed record AuditWriteRequest(
    string Action,
    AuditOperation Operation,
    string EntityType,
    string? EntityId,
    bool IsSuccessful,
    object? Metadata = null,
    string? UserId = null,
    string? RemoteIpAddress = null);
