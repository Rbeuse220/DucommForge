using DucommForge.Composition;
using DucommForge.Data;
using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace DucommForge;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = AppHost.BuildHost();
        _host.Start();

        EnsureDatabase();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    private void EnsureDatabase()
    {
        using var scope = _host!.Services.CreateScope();
        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<DucommForgeDbContext>>();

        using var db = factory.CreateDbContext();

        db.Database.Migrate();

        // Ensure base dispatch center exists
        if (!db.DispatchCenters.Any(x => x.Code == "DUCOMM"))
        {
            db.DispatchCenters.Add(new DispatchCenter
            {
                Code = "DUCOMM",
                Name = "DuPage Public Safety Communications",
                Active = true
            });

            db.SaveChanges();
        }

#if DEBUG
        SeedDevData(factory);
#endif
    }

#if DEBUG
    private static void SeedDevData(IDbContextFactory<DucommForgeDbContext> factory)
    {
        using var db = factory.CreateDbContext();

        if (db.Agencies.Any())
            return;

        var ducomm = db.DispatchCenters.First(x => x.Code == "DUCOMM");

        var alt = new DispatchCenter
        {
            Code = "ALT",
            Name = "Alternate Center",
            Active = true
        };

        db.DispatchCenters.Add(alt);
        db.SaveChanges();

        db.AppSettings.Add(new AppSetting
        {
            Key = "CurrentDispatchCenterCode",
            Value = "DUCOMM"
        });

        db.Agencies.AddRange(
            new Agency
            {
                DispatchCenterId = ducomm.DispatchCenterId,
                Short = "BAF",
                Name = "Bartlett Fire",
                Type = "Fire",
                Owned = true,
                Active = true
            },
            new Agency
            {
                DispatchCenterId = ducomm.DispatchCenterId,
                Short = "CSF",
                Name = "Carol Stream Fire",
                Type = "Fire",
                Owned = false,
                Active = true
            },
            new Agency
            {
                DispatchCenterId = alt.DispatchCenterId,
                Short = "ZZF",
                Name = "Zeta Zone Fire",
                Type = "Fire",
                Owned = false,
                Active = true
            }
        );

        db.SaveChanges();
    }
#endif
}