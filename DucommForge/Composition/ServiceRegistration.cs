using DucommForge.Data;
using DucommForge.Services.Auth;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels;
using DucommForge.ViewModels.Agencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DucommForge.Composition;

public static class ServiceRegistration
{
    public static void Configure(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddDbContextFactory<DucommForgeDbContext>(opt =>
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

        services.AddTransient<AgencyQueryService>();
        services.AddTransient<AgencyDetailQueryService>();
        services.AddTransient<AgencyCommandService>();
        services.AddTransient<CurrentDispatchCenterService>();

        services.AddTransient<IAgencyDetailViewModelFactory, AgencyDetailViewModelFactory>();
        services.AddTransient<IAgencyEditViewModelFactory, AgencyEditViewModelFactory>();
        services.AddTransient<IAgencyCreateViewModelFactory, AgencyCreateViewModelFactory>();

        services.AddTransient<AgenciesViewModel>();
        services.AddTransient<AgencyCreateViewModel>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }
}