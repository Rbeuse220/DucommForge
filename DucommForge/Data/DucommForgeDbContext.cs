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

    // DI constructor (used when created via AddDbContext)
    public DucommForgeDbContext(DbContextOptions<DucommForgeDbContext> options)
        : base(options)
    {
    }

    // Parameterless constructor fallback (only if something new's it up manually)
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
        // DispatchCenter: Code unique
        modelBuilder.Entity<DispatchCenter>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<Agency>()
            .HasOne(a => a.DispatchCenter)
            .WithMany()
            .HasForeignKey(a => a.DispatchCenterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Station>()
            .HasOne(s => s.Agency)
            .WithMany()
            .HasForeignKey(s => s.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Unit>()
            .HasOne(u => u.Station)
            .WithMany()
            .HasForeignKey(u => u.StationKey)
            .OnDelete(DeleteBehavior.Cascade);
    }
}