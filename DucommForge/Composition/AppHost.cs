using Microsoft.Extensions.Hosting;

namespace DucommForge.Composition;

public static class AppHost
{
    public static IHost BuildHost() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices(ServiceRegistration.Configure)
            .Build();
}