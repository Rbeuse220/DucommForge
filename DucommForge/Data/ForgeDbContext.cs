using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DucommForge.Data;

public class ForgeDbContext : DbContext
{
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<DispatchCenter> DispatchCenters => Set<DispatchCenter>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Unit> Units => Set<Unit>();
    public string DbPath { get; }

    public ForgeDbContext()
    {
        var dir = AppPaths.AppDataDir;
        Directory.CreateDirectory(dir);
        DbPath = Path.Combine(dir, "ducomm_forge.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Settings
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

            e.HasIndex(x => x.Code)
             .IsUnique();

            e.Property(x => x.Code)
             .IsRequired();

            e.Property(x => x.Name)
             .IsRequired();

            e.Property(x => x.Active)
             .HasDefaultValue(true);
        });

        // Agency
        modelBuilder.Entity<Agency>(e =>
        {
            e.HasKey(x => x.Short);

            e.Property(x => x.Short)
             .IsRequired();

            e.Property(x => x.Name)
             .IsRequired(false);

            e.Property(x => x.Type)
             .IsRequired();

            e.Property(x => x.Owned)
             .HasDefaultValue(false);

            e.Property(x => x.Active)
             .HasDefaultValue(true);

            e.HasOne(a => a.DispatchCenter)
             .WithMany()
             .HasForeignKey(a => a.DispatchCenterId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(a => new { a.DispatchCenterId, a.Short })
             .IsUnique();
        });

        // Station
        modelBuilder.Entity<Station>(e =>
        {
            e.HasKey(x => x.StationId);

            e.Property(x => x.StationId)
             .IsRequired();

            e.Property(x => x.AgencyShort)
             .IsRequired();

            e.Property(x => x.Esz)
             .IsRequired(false);

            e.Property(x => x.Active)
             .HasDefaultValue(true);

            e.HasOne(s => s.Agency)
             .WithMany()
             .HasForeignKey(s => s.AgencyShort)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => new { s.AgencyShort, s.StationId })
             .IsUnique();
        });

        // Unit
        modelBuilder.Entity<Unit>(e =>
        {
            e.HasKey(x => x.UnitId);

            e.Property(x => x.UnitId)
             .IsRequired();

            e.Property(x => x.StationId)
             .IsRequired();

            e.Property(x => x.Type)
             .IsRequired();

            e.Property(x => x.Jump)
             .HasDefaultValue(false);

            e.Property(x => x.Active)
             .HasDefaultValue(true);

            e.HasOne(u => u.Station)
             .WithMany()
             .HasForeignKey(u => u.StationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(u => new { u.StationId, u.UnitId })
             .IsUnique();
        });
    }
}