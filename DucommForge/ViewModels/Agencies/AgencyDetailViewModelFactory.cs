using DucommForge.Data;
using DucommForge.Services.Navigation;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyDetailViewModelFactory : IAgencyDetailViewModelFactory
{
    private readonly AgencyDetailQueryService _query;
    private readonly INavigationService _navigation;

    public AgencyDetailViewModelFactory(AgencyDetailQueryService query, INavigationService navigation)
    {
        _query = query;
        _navigation = navigation;
    }

    public AgencyDetailViewModel Create(int agencyId)
    {
        return new AgencyDetailViewModel(_query, _navigation, agencyId);
    }
}