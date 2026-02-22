using System.Collections.ObjectModel;
using System.Linq;
using DucommForge.Data;
using DucommForge.Services.Auth;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgenciesViewModel : ViewModelBase, INavigationAware
{
    private readonly AgencyQueryService _query;
    private readonly INavigationService _navigation;
    private readonly IAuthorizationService _auth;
    private readonly IAgencyDetailViewModelFactory _detailFactory;

    public AgenciesViewModel(
        AgencyQueryService query,
        INavigationService navigation,
        IAuthorizationService auth,
        IAgencyDetailViewModelFactory detailFactory)
    {
        _query = query;
        _navigation = navigation;
        _auth = auth;
        _detailFactory = detailFactory;

        Rows = new ObservableCollection<AgencyRowViewModel>();

        RefreshCommand = new RelayCommand(async () => await RefreshAsync());

        ToggleScopeCommand = new RelayCommand(async () =>
        {
            Scope = Scope == AgencyScope.CurrentDispatchCenter
                ? AgencyScope.AllDispatchCenters
                : AgencyScope.CurrentDispatchCenter;

            await RefreshAsync();
        });

        _ = RefreshAsync();
    }

    public string Title => "Agencies";

    public ObservableCollection<AgencyRowViewModel> Rows { get; }

    private AgencyScope _scope = AgencyScope.CurrentDispatchCenter;
    public AgencyScope Scope
    {
        get => _scope;
        set => SetProperty(ref _scope, value);
    }

    private string? _dispatchCenterScopeCodeOverride;
    public string? DispatchCenterScopeCodeOverride
    {
        get => _dispatchCenterScopeCodeOverride;
        set => SetProperty(ref _dispatchCenterScopeCodeOverride, value);
    }

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private bool _activeOnly;
    public bool ActiveOnly
    {
        get => _activeOnly;
        set => SetProperty(ref _activeOnly, value);
    }

    private AgencyRowViewModel? _selected;
    public AgencyRowViewModel? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ToggleScopeCommand { get; }

    private int? _pendingSelectAgencyId;

    public void OnNavigatedTo(NavigationState? state)
    {
        if (state == null) return;

        if (state.AgencyScope is int scopeValue && (scopeValue == 0 || scopeValue == 1))
        {
            Scope = (AgencyScope)scopeValue;
        }

        DispatchCenterScopeCodeOverride = state.DispatchCenterScopeCode;
        SearchText = state.SearchText;
        ActiveOnly = state.ActiveOnly ?? false;

        _pendingSelectAgencyId = state.SelectedAgencyId;

        _ = RefreshAsync();
    }

    private NavigationState CaptureReturnState(int? selectedAgencyId)
    {
        return new NavigationState
        {
            AgencyScope = (int)Scope,
            DispatchCenterScopeCode = DispatchCenterScopeCodeOverride,
            SearchText = SearchText,
            ActiveOnly = ActiveOnly,
            SelectedAgencyId = selectedAgencyId
        };
    }

    private async Task RefreshAsync()
    {
        var items = await _query.GetAgenciesAsync(
            scope: Scope,
            dispatchCenterScopeCodeOverride: DispatchCenterScopeCodeOverride,
            searchText: SearchText,
            activeOnly: ActiveOnly);

        Rows.Clear();

        foreach (var item in items)
        {
            var detailsCmd = new RelayCommand(() => NavigateDetails(item.AgencyId));
            var editCmd = new RelayCommand(() => NavigateEdit(item.AgencyId));
            var canEdit = _auth.CanEditAgency(item.DispatchCenterId);

            Rows.Add(new AgencyRowViewModel(item, canEdit, detailsCmd, editCmd));
        }

        if (_pendingSelectAgencyId is int id)
        {
            Selected = Rows.FirstOrDefault(r => r.AgencyId == id);
            _pendingSelectAgencyId = null;
        }
    }

    private void NavigateDetails(int agencyId)
    {
        Selected = Rows.FirstOrDefault(r => r.AgencyId == agencyId) ?? Selected;

        var returnState = CaptureReturnState(agencyId);

        var vm = _detailFactory.Create(agencyId);
        _navigation.Navigate(vm, returnState);
    }

    private void NavigateEdit(int agencyId)
    {
        // Not implemented yet.
    }
}