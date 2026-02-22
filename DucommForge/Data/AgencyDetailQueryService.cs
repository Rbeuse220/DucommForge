using Microsoft.EntityFrameworkCore;

namespace DucommForge.Data;

public sealed class AgencyDetailQueryService
{
    private readonly IDbContextFactory<DucommForgeDbContext> _dbFactory;

    public AgencyDetailQueryService(IDbContextFactory<DucommForgeDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<AgencyDetailItem?> GetAgencyAsync(int agencyId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Agencies
            .AsNoTracking()
            .Include(a => a.DispatchCenter)
            .Where(a => a.AgencyId == agencyId)
            .Select(a => new AgencyDetailItem
            {
                AgencyId = a.AgencyId,
                DispatchCenterId = a.DispatchCenterId,
                Short = a.Short,
                Name = a.Name,
                Type = a.Type,
                Owned = a.Owned,
                Active = a.Active,
                DispatchCenterCode = a.DispatchCenter != null ? a.DispatchCenter.Code : string.Empty,
                DispatchCenterName = a.DispatchCenter != null ? a.DispatchCenter.Name : string.Empty
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}