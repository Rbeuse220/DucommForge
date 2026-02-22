using Microsoft.EntityFrameworkCore;

namespace DucommForge.Data;

public sealed class AgencyQueryService(IDbContextFactory<DucommForgeDbContext> dbFactory)
{
    public async Task<List<AgencyListItem>> GetAgenciesAsync(
        AgencyScope scope,
        string? searchText,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<Entities.Agency> query = db.Agencies
            .AsNoTracking()
            .Include(a => a.DispatchCenter);

        if (scope == AgencyScope.CurrentDispatchCenter)
        {
            var centerCode = await db.AppSettings
                .AsNoTracking()
                .Where(s => s.Key == "CurrentDispatchCenterCode")
                .Select(s => s.Value)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(centerCode))
            {
                query = query.Where(a => a.DispatchCenter != null && a.DispatchCenter.Code == centerCode);
            }
        }

        if (activeOnly)
        {
            query = query.Where(a => a.Active);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(a =>
                a.Short.Contains(searchText) ||
                a.Name.Contains(searchText));
        }

        return await query
            .OrderBy(a => a.Short)
            .Select(a => new AgencyListItem
            {
                AgencyId = a.AgencyId,
                DispatchCenterId = a.DispatchCenterId,
                Short = a.Short,
                Name = a.Name,
                Type = a.Type,
                Owned = a.Owned,
                Active = a.Active,
                DispatchCenterCode = a.DispatchCenter != null ? a.DispatchCenter.Code : string.Empty
            })
            .ToListAsync(cancellationToken);
    }
}