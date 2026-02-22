using DucommForge.Data;
using DucommForge.Services.Auth;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DucommForge.Composition;

public static class ServiceRegistration
{
    public static void Configure(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddDbContext<DucommForgeDbContext>(opt =>
        {
            var dbPath = AppPaths.GetDbPath();
            opt.UseSqlite($"Data Source={dbPath}");
        });

        services.AddSingleton(new CurrentUserContext
        {
            Username = Environment.UserName,
            Role = UserRole.Admin,
            CanEditAllDispatchCenters = true
        });

        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }
}