using CEI.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace CEI.Infrastructure.Persistence;

public sealed class ApplicationDbContextInitialiser(
    ApplicationDbContext dbContext,
    DatabaseSeeder databaseSeeder)
{
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await databaseSeeder.SeedAsync(cancellationToken);
    }
}
