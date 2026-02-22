using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
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

    private readonly DispatcherTimer _refreshTimer;

    private string _lastQueryKey = string.Empty;
    private int? _pendingSelectAgencyId;

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

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _refreshTimer.Tick += async (_, _) =>
        {
            _refreshTimer.Stop();
            await RefreshAsync();
        };

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);

        ToggleScopeCommand = new AsyncRelayCommand(async () =>
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
        set
        {
            if (!SetProperty(ref _scope, value)) return;
            Raise(nameof(ScopeLabel));
            ScheduleRefresh();
        }
    }

    public string ScopeLabel => Scope switch
    {
        AgencyScope.CurrentDispatchCenter => "Current dispatch center",
        AgencyScope.AllDispatchCenters => "All dispatch centers",
        _ => Scope.ToString()
    };

    private string? _dispatchCenterScopeCodeOverride;
    public string? DispatchCenterScopeCodeOverride
    {
        get => _dispatchCenterScopeCodeOverride;
        set
        {
            if (!SetProperty(ref _dispatchCenterScopeCodeOverride, value)) return;
            ScheduleRefresh();
        }
    }

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (!SetProperty(ref _searchText, value)) return;
            ScheduleRefresh();
        }
    }

    private bool _activeOnly = true;
    public bool ActiveOnly
    {
        get => _activeOnly;
        set
        {
            if (!SetProperty(ref _activeOnly, value)) return;
            ScheduleRefresh();
        }
    }

    private AgencyRowViewModel? _selected;
    public AgencyRowViewModel? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand ToggleScopeCommand { get; }

    private void ScheduleRefresh()
    {
        _refreshTimer.Stop();
        _refreshTimer.Start();
    }

    public void OnNavigatedTo(NavigationState? state)
    {
        if (state == null) return;

        if (state.AgencyScope is int scopeValue && (scopeValue == 0 || scopeValue == 1))
        {
            Scope = (AgencyScope)scopeValue;
        }

        DispatchCenterScopeCodeOverride = state.DispatchCenterScopeCode;
        SearchText = state.SearchText;
        ActiveOnly = state.ActiveOnly ?? true;

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
        var queryKey = $"{(int)Scope}|{DispatchCenterScopeCodeOverride}|{SearchText}|{ActiveOnly}";
        if (queryKey == _lastQueryKey && _pendingSelectAgencyId == null)
            return;

        _lastQueryKey = queryKey;

        var items = await _query.GetAgenciesAsync(
            scope: Scope,
            dispatchCenterScopeCodeOverride: DispatchCenterScopeCodeOverride,
            searchText: SearchText,
            activeOnly: ActiveOnly);

        Rows.Clear();

        foreach (var item in items)
        {
            var canEdit = _auth.CanEditAgency(item.DispatchCenterId);

            // AgencyRowViewModel ctor expects AsyncRelayCommand for args 3/4 in your build.
            var detailsCmd = new AsyncRelayCommand(() => NavigateDetails(item.AgencyId));
            var editCmd = new AsyncRelayCommand(() => NavigateEdit(item.AgencyId));

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
        Raise(nameof(Selected));
    }

    private void NavigateEdit(int agencyId)
    {
        // Not implemented yet.
    }
}