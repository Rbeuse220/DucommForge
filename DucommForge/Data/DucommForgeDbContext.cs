using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DucommForge.Data;

public sealed class DucommForgeDbContext : DbContext
{
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<DispatchCenter> DispatchCenters => Set<DispatchCenter>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Unit> Units => Set<Unit>();

    public DucommForgeDbContext(DbContextOptions<DucommForgeDbContext> options)
        : base(options)
    {
    }

    // Design-time fallback for EF tooling. Runtime should use DI factory.
    public DucommForgeDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var dbPath = AppPaths.GetDbPath();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -----------------------------
        // AppSettings
        // -----------------------------
        modelBuilder.Entity<AppSetting>(e =>
        {
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Value).IsRequired();

            e.HasIndex(x => x.Key).IsUnique();
        });

        // -----------------------------
        // DispatchCenters
        // -----------------------------
        modelBuilder.Entity<DispatchCenter>(e =>
        {
            e.Property(x => x.Code).IsRequired();
            e.Property(x => x.Name).IsRequired();

            e.HasIndex(x => x.Code).IsUnique();
        });

        // -----------------------------
        // Agencies
        // -----------------------------
        modelBuilder.Entity<Agency>(e =>
        {
            e.Property(x => x.Short).IsRequired();
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Type).IsRequired();

            e.HasOne(x => x.DispatchCenter)
                .WithMany()
                .HasForeignKey(x => x.DispatchCenterId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Performance index for FK
            e.HasIndex(x => x.DispatchCenterId);

            // Uniqueness constraint
            e.HasIndex(x => new { x.DispatchCenterId, x.Short }).IsUnique();
        });

        // -----------------------------
        // Stations
        // -----------------------------
        modelBuilder.Entity<Station>(e =>
        {
            e.Property(x => x.StationId).IsRequired();

            e.HasOne(x => x.Agency)
                .WithMany()
                .HasForeignKey(x => x.AgencyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Performance index for FK
            e.HasIndex(x => x.AgencyId);

            // Uniqueness constraint
            e.HasIndex(x => new { x.AgencyId, x.StationId }).IsUnique();
        });

        // -----------------------------
        // Units
        // -----------------------------
        modelBuilder.Entity<Unit>(e =>
        {
            e.Property(x => x.UnitId).IsRequired();
            e.Property(x => x.Type).IsRequired();

            e.HasOne(x => x.Station)
                .WithMany()
                .HasForeignKey(x => x.StationKey)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Performance index for FK
            e.HasIndex(x => x.StationKey);

            // Uniqueness constraint
            e.HasIndex(x => new { x.StationKey, x.UnitId }).IsUnique();
        });
    }
}