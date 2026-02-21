using System.Windows;
using DucommForge.Data;
using System.Linq;

namespace DucommForge;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using var db = new ForgeDbContext();
        //db.Database.EnsureCreated();

        // Seed the primary dispatch center (DUCOMM) if missing
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