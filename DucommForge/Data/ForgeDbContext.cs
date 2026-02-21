using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DucommForge.Data;

public class ForgeDbContext : DbContext
{
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<DispatchCenter> DispatchCenters => Set<DispatchCenter>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Station> Stations => Set<Station>();

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
        modelBuilder.Entity<AppSetting>()
            .HasKey(x => x.Key);

        // DispatchCenter
        modelBuilder.Entity<DispatchCenter>()
            .HasKey(x => x.DispatchCenterId);

        modelBuilder.Entity<DispatchCenter>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<DispatchCenter>()
            .Property(x => x.Code)
            .IsRequired();

        modelBuilder.Entity<DispatchCenter>()
            .Property(x => x.Name)
            .IsRequired();

        // Agency
        modelBuilder.Entity<Agency>()
            .HasKey(x => x.Short);

        modelBuilder.Entity<Agency>()
            .Property(x => x.Short)
            .IsRequired();

        modelBuilder.Entity<Agency>()
            .HasOne(a => a.DispatchCenter)
            .WithMany()
            .HasForeignKey(a => a.DispatchCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Station
        modelBuilder.Entity<Station>()
            .HasKey(x => x.StationId);

        modelBuilder.Entity<Station>()
            .Property(x => x.StationId)
            .IsRequired();

        modelBuilder.Entity<Station>()
            .Property(x => x.AgencyShort)
            .IsRequired();

        modelBuilder.Entity<Station>()
            .Property(x => x.Active)
            .HasDefaultValue(true);

        modelBuilder.Entity<Station>()
            .HasOne(s => s.Agency)
            .WithMany()
            .HasForeignKey(s => s.AgencyShort)
            .OnDelete(DeleteBehavior.Restrict);
    }
}