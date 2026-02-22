using DucommForge.Data;
using DucommForge.Services.Auth;
using DucommForge.Services.Navigation;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyDetailViewModelFactory : IAgencyDetailViewModelFactory
{
    private readonly AgencyDetailQueryService _query;
    private readonly INavigationService _navigation;
    private readonly IAuthorizationService _auth;
    private readonly IAgencyEditViewModelFactory _editFactory;

    public AgencyDetailViewModelFactory(
        AgencyDetailQueryService query,
        INavigationService navigation,
        IAuthorizationService auth,
        IAgencyEditViewModelFactory editFactory)
    {
        _query = query;
        _navigation = navigation;
        _auth = auth;
        _editFactory = editFactory;
    }

    public AgencyDetailViewModel Create(int agencyId)
    {
        return new AgencyDetailViewModel(_query, _navigation, _auth, _editFactory, agencyId);
    }
}