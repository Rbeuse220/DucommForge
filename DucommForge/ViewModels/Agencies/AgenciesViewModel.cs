using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using DucommForge.Data;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgenciesViewModel : ViewModelBase, INavigationAware
{
    private readonly AgencyQueryService _query;
    private readonly INavigationService _navigation;
    private readonly IAgencyDetailViewModelFactory _detailFactory;

    private readonly DispatcherTimer _refreshTimer;

    private string _lastQueryKey = string.Empty;
    private int? _pendingSelectAgencyId;

    public AgenciesViewModel(
        AgencyQueryService query,
        INavigationService navigation,
        IAgencyDetailViewModelFactory detailFactory)
    {
        _query = query;
        _navigation = navigation;
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
        if (state == null)
        {
            _ = RefreshAsync();
            return;
        }

        if (state.AgencyScope is int scopeValue && (scopeValue == 0 || scopeValue == 1))
        {
            Scope = (AgencyScope)scopeValue;
        }

        SearchText = state.SearchText;
        ActiveOnly = state.ActiveOnly ?? true;

        // Always prefer selecting the edited item when present.
        if (state.EditedAgencyId is int editedId)
        {
            _pendingSelectAgencyId = editedId;
        }
        else
        {
            _pendingSelectAgencyId = state.SelectedAgencyId;
        }

        if (TryApplyEditStateToList(state))
        {
            return;
        }

        _ = RefreshAsync();
    }

    private bool TryApplyEditStateToList(NavigationState state)
    {
        if (state.EditedAgencyId is not int id)
            return false;

        if (state.EditedName == null ||
            state.EditedType == null ||
            state.EditedOwned == null ||
            state.EditedActive == null)
            return false;

        var row = Rows.FirstOrDefault(r => r.AgencyId == id);

        var shouldBeIncluded = MatchesFilters(
            shortCode: row?.Short,
            name: state.EditedName,
            active: state.EditedActive.Value);

        if (row != null)
        {
            if (shouldBeIncluded)
            {
                row.ApplyEdits(state.EditedName, state.EditedType, state.EditedOwned.Value, state.EditedActive.Value);
                Selected = row;
            }
            else
            {
                var wasSelected = ReferenceEquals(Selected, row);
                Rows.Remove(row);
                if (wasSelected) Selected = null;
            }

            return true;
        }

        // Row missing: if it should now be included, fall back to normal refresh
        // so the row can be loaded with all list-only fields (ex: DispatchCenterCode).
        if (shouldBeIncluded)
        {
            _pendingSelectAgencyId = id;
            return false;
        }

        // Missing and excluded: nothing to do, and no refresh required.
        return true;
    }

    private bool MatchesFilters(string? shortCode, string name, bool active)
    {
        if (ActiveOnly && !active)
            return false;

        var s = SearchText;
        if (string.IsNullOrWhiteSpace(s))
            return true;

        var needle = s.Trim();

        if (!string.IsNullOrEmpty(shortCode) &&
            shortCode.Contains(needle, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrEmpty(name) &&
            name.Contains(needle, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private NavigationState CaptureReturnState(int? selectedAgencyId)
    {
        return new NavigationState
        {
            AgencyScope = (int)Scope,
            SearchText = SearchText,
            ActiveOnly = ActiveOnly,
            SelectedAgencyId = selectedAgencyId
        };
    }

    private async Task RefreshAsync()
    {
        var queryKey = $"{(int)Scope}|{SearchText}|{ActiveOnly}";
        if (queryKey == _lastQueryKey && _pendingSelectAgencyId == null)
            return;

        _lastQueryKey = queryKey;

        var items = await _query.GetAgenciesAsync(
            scope: Scope,
            searchText: SearchText,
            activeOnly: ActiveOnly);

        Rows.Clear();

        foreach (var item in items)
        {
            var detailsCmd = new AsyncRelayCommand(() => NavigateDetails(item.AgencyId));
            Rows.Add(new AgencyRowViewModel(item, detailsCmd));
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
}