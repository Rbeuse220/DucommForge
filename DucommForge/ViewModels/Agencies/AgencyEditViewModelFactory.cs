using DucommForge.Data;
using DucommForge.Services.Navigation;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyEditViewModelFactory(
    AgencyDetailQueryService query,
    AgencyCommandService commands,
    INavigationService navigation) : IAgencyEditViewModelFactory
{
    public AgencyEditViewModel Create(int agencyId)
    {
        return new AgencyEditViewModel(query, commands, navigation, agencyId);
    }
}