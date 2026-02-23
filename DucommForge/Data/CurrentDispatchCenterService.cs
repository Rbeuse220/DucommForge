using Microsoft.EntityFrameworkCore;

namespace DucommForge.Data;

public sealed class CurrentDispatchCenterService(IDbContextFactory<DucommForgeDbContext> dbFactory)
{
    public async Task<DispatchCenterInfo?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var centerCode = await db.AppSettings
            .AsNoTracking()
            .Where(s => s.Key == "CurrentDispatchCenterCode")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(centerCode))
            return null;

        return await db.DispatchCenters
            .AsNoTracking()
            .Where(dc => dc.Code == centerCode)
            .Select(dc => new DispatchCenterInfo
            {
                DispatchCenterId = dc.DispatchCenterId,
                Code = dc.Code,
                Name = dc.Name,
                Active = dc.Active
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}