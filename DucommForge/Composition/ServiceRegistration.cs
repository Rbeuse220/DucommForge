using DucommForge.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DucommForge.Composition;

public static class ServiceRegistration
{
    public static void Configure(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddDbContext<DucommForgeDbContext>(opt =>
        {
            var dbPath = AppPaths.DatabasePath; // your existing helper
            opt.UseSqlite($"Data Source={dbPath}");
        });

        services.AddSingleton<MainWindow>();
    }
}