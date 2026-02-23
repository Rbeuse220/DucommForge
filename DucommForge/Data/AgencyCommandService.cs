using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DucommForge.Data;

public sealed class AgencyCommandService(IDbContextFactory<DucommForgeDbContext> dbFactory)
{
    public sealed class CreateAgencyResult
    {
        public bool Success { get; init; }
        public int AgencyId { get; init; }
        public string? Error { get; init; }
    }

    public async Task<CreateAgencyResult> CreateAgencyAsync(
        int dispatchCenterId,
        string shortCode,
        string name,
        string type,
        bool owned,
        bool active,
        CancellationToken cancellationToken = default)
    {
        shortCode = (shortCode ?? string.Empty).Trim().ToUpperInvariant();
        name = (name ?? string.Empty).Trim();
        type = (type ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(shortCode))
            return new CreateAgencyResult { Success = false, Error = "Short is required." };

        if (string.IsNullOrWhiteSpace(name))
            return new CreateAgencyResult { Success = false, Error = "Name is required." };

        if (string.IsNullOrWhiteSpace(type))
            return new CreateAgencyResult { Success = false, Error = "Type is required." };

        if (shortCode.Length > 10)
            return new CreateAgencyResult { Success = false, Error = "Short is too long." };

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var exists = await db.Agencies
            .AsNoTracking()
            .AnyAsync(a => a.DispatchCenterId == dispatchCenterId && a.Short == shortCode, cancellationToken);

        if (exists)
            return new CreateAgencyResult { Success = false, Error = $"Short '{shortCode}' already exists for this dispatch center." };

        var agency = new Agency
        {
            DispatchCenterId = dispatchCenterId,
            Short = shortCode,
            Name = name,
            Type = type,
            Owned = owned,
            Active = active
        };

        db.Agencies.Add(agency);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateAgencyResult { Success = true, AgencyId = agency.AgencyId };
    }

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