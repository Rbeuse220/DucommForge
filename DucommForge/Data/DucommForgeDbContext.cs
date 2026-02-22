using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DucommForge.Data;

public class DucommForgeDbContext : DbContext
{
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<DispatchCenter> DispatchCenters => Set<DispatchCenter>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Unit> Units => Set<Unit>();
    public string DbPath { get; }

    public DucommForgeDbContext()
    {
        var dir = AppPaths.AppDataDir;
        Directory.CreateDirectory(dir);
        DbPath = Path.Combine(dir, "ducomm_forge.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AppSetting
        modelBuilder.Entity<AppSetting>(e =>
        {
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Value).IsRequired(false);
        });

        // DispatchCenter
        modelBuilder.Entity<DispatchCenter>(e =>
        {
            e.HasKey(x => x.DispatchCenterId);

            e.Property(x => x.Code).IsRequired();
            e.Property(x => x.Name).IsRequired();

            e.Property(x => x.Active).HasDefaultValue(true);

            e.HasIndex(x => x.Code).IsUnique();
        });

        // Agency
        modelBuilder.Entity<Agency>(e =>
        {
            e.HasKey(x => x.AgencyId);

            e.Property(x => x.Short).IsRequired();
            e.Property(x => x.Type).IsRequired();

            e.Property(x => x.Owned).HasDefaultValue(true);
            e.Property(x => x.Active).HasDefaultValue(true);

            e.HasOne(x => x.DispatchCenter)
             .WithMany()
             .HasForeignKey(x => x.DispatchCenterId)
             .OnDelete(DeleteBehavior.Restrict);

            // Unique per DispatchCenter
            e.HasIndex(x => new { x.DispatchCenterId, x.Short })
             .IsUnique();

            // Helpful for scope filtering
            e.HasIndex(x => x.DispatchCenterId);
        });

        // Station
        modelBuilder.Entity<Station>(e =>
        {
            e.HasKey(x => x.StationKey);

            e.Property(x => x.StationId).IsRequired();
            e.Property(x => x.Active).HasDefaultValue(true);

            e.HasOne(x => x.Agency)
             .WithMany()
             .HasForeignKey(x => x.AgencyId)
             .OnDelete(DeleteBehavior.Restrict);

            // Unique per Agency
            e.HasIndex(x => new { x.AgencyId, x.StationId })
             .IsUnique();

            e.HasIndex(x => x.AgencyId);
        });

        // Unit
        modelBuilder.Entity<Unit>(e =>
        {
            e.HasKey(x => x.UnitKey);

            e.Property(x => x.UnitId).IsRequired();
            e.Property(x => x.Type).IsRequired();

            e.Property(x => x.Jump).HasDefaultValue(false);
            e.Property(x => x.Active).HasDefaultValue(true);

            e.HasOne(x => x.Station)
             .WithMany()
             .HasForeignKey(x => x.StationKey)
             .OnDelete(DeleteBehavior.Restrict);

            // Unique per Station
            e.HasIndex(x => new { x.StationKey, x.UnitId })
             .IsUnique();

            e.HasIndex(x => x.StationKey);
        });
    }
}