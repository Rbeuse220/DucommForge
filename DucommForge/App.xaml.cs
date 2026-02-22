using System.Linq;
using System.Windows;
using DucommForge.Composition;
using DucommForge.Data;
using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        var db = scope.ServiceProvider.GetRequiredService<DucommForgeDbContext>();

        // Apply migrations (creates tables)
        db.Database.Migrate();

        // Seed DUCOMM if missing
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
    }
}