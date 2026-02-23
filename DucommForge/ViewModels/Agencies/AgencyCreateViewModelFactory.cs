using Microsoft.Extensions.DependencyInjection;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyCreateViewModelFactory(IServiceProvider services) : IAgencyCreateViewModelFactory
{
    public AgencyCreateViewModel Create() => services.GetRequiredService<AgencyCreateViewModel>();
}