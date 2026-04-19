using System.Text.Json;
using CEI.Application.Common.Interfaces;
using CEI.Domain.Audit;

namespace CEI.Application.Security;

public sealed class AuditWriter(IApplicationDbContext dbContext) : IAuditWriter
{
    public async Task WriteAsync(AuditWriteRequest request, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            Action = request.Action,
            Operation = request.Operation,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            IsSuccessful = request.IsSuccessful,
            MetadataJson = request.Metadata is null ? null : JsonSerializer.Serialize(request.Metadata),
            UserId = request.UserId,
            RemoteIpAddress = request.RemoteIpAddress,
            OccurredOnUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
