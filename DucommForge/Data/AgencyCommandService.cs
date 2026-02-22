using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DucommForge.Data;

public sealed class AgencyCommandService(IDbContextFactory<DucommForgeDbContext> dbFactory)
{
    public async Task<bool> UpdateAgencyAsync(
        int agencyId,
        string name,
        string type,
        bool owned,
        bool active,
        CancellationToken cancellationToken = default)
    {
        name = (name ?? string.Empty).Trim();
        type = (type ?? string.Empty).Trim();

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var agency = await db.Agencies
            .Where(a => a.AgencyId == agencyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (agency == null)
            return false;

        agency.Name = name;
        agency.Type = type;
        agency.Owned = owned;
        agency.Active = active;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}