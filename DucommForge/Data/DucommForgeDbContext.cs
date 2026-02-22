using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DucommForge.Data;

public class DucommForgeDbContext : DbContext
{
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<DispatchCenter> DispatchCenters => Set<DispatchCenter>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Unit> Units => Set<Unit>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = AppPaths.GetDbPath();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DispatchCenter: Code unique
        modelBuilder.Entity<DispatchCenter>()
            .HasIndex(x => x.Code)
            .IsUnique();

        // Agency: (DispatchCenterId, Short) unique
        modelBuilder.Entity<Agency>()
            .HasIndex(x => new { x.DispatchCenterId, x.Short })
            .IsUnique();

        // Station: (AgencyId, StationId) unique
        modelBuilder.Entity<Station>()
            .HasIndex(x => new { x.AgencyId, x.StationId })
            .IsUnique();

        // Unit: (StationKey, UnitId) unique
        modelBuilder.Entity<Unit>()
            .HasIndex(x => new { x.StationKey, x.UnitId })
            .IsUnique();
    }
}