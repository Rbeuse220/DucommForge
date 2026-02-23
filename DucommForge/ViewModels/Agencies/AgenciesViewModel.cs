using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
    private readonly CurrentDispatchCenterService _currentCenter;
    private readonly IAuthorizationService _auth;
    private readonly INavigationService _navigation;
    private readonly IAgencyDetailViewModelFactory _detailFactory;
    private readonly IAgencyCreateViewModelFactory _createFactory;

    private readonly DispatcherTimer _refreshTimer;

    private string _lastQueryKey = string.Empty;
    private int? _pendingSelectAgencyId;

    private CancellationTokenSource? _refreshCts;
    private CancellationTokenSource? _centerCts;

    private DispatchCenterInfo? _center;

    public AgenciesViewModel(
        AgencyQueryService query,
        CurrentDispatchCenterService currentCenter,
        IAuthorizationService auth,
        INavigationService navigation,
        IAgencyDetailViewModelFactory detailFactory,
        IAgencyCreateViewModelFactory createFactory)
    {
        _query = query;
        _currentCenter = currentCenter;
        _auth = auth;
        _navigation = navigation;
        _detailFactory = detailFactory;
        _createFactory = createFactory;

        Rows = new ObservableCollection<AgencyRowViewModel>();

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _refreshTimer.Tick += (_, _) =>
        {
            _refreshTimer.Stop();
            _ = RefreshAsync();
        };

        RefreshCommand = new AsyncRelayCommand(() => RefreshAsync(force: true));
        ToggleScopeCommand = new AsyncRelayCommand(ToggleScopeAsync);
        CreateCommand = new AsyncRelayCommand(NavigateCreateAsync, () => CanCreate);
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

    private bool _canCreate;
    public bool CanCreate
    {
        get => _canCreate;
        private set
        {
            if (!SetProperty(ref _canCreate, value)) return;
            CreateCommand.RaiseCanExecuteChanged();
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand ToggleScopeCommand { get; }
    public AsyncRelayCommand CreateCommand { get; }

    private Task ToggleScopeAsync()
    {
        Scope = Scope == AgencyScope.CurrentDispatchCenter
            ? AgencyScope.AllDispatchCenters
            : AgencyScope.CurrentDispatchCenter;

        ScheduleRefresh();
        return Task.CompletedTask;
    }

    private void ScheduleRefresh()
    {
        _refreshTimer.Stop();
        _refreshTimer.Start();
    }

    public void OnNavigatedTo(NavigationState? state)
    {
        EnsureCenterLoaded();

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

        if (state.CreatedAgencyId is int createdId)
        {
            _pendingSelectAgencyId = createdId;
        }
        else if (state.EditedAgencyId is int editedId)
        {
            _pendingSelectAgencyId = editedId;
        }
        else
        {
            _pendingSelectAgencyId = state.SelectedAgencyId;
        }

        if (TryApplyCreateStateToList(state))
            return;

        if (TryApplyEditStateToList(state))
            return;

        _ = RefreshAsync();
    }

    private void EnsureCenterLoaded()
    {
        if (_center != null)
        {
            CanCreate = _auth.CanCreateAgency(_center.DispatchCenterId);
            return;
        }

        var cts = new CancellationTokenSource();
        var prior = Interlocked.Exchange(ref _centerCts, cts);
        prior?.Cancel();
        prior?.Dispose();

        _ = LoadCenterAsync(cts.Token);
    }

    private async Task LoadCenterAsync(CancellationToken cancellationToken)
    {
        try
        {
            var center = await _currentCenter.GetCurrentAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return;

            _center = center;
            if (center != null)
            {
                CanCreate = _auth.CanCreateAgency(center.DispatchCenterId);
            }
            else
            {
                CanCreate = false;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected.
        }
        catch
        {
            CanCreate = false;
        }
        finally
        {
            var cts = Interlocked.Exchange(ref _centerCts, null);
            cts?.Dispose();
        }
    }

    private bool TryApplyCreateStateToList(NavigationState state)
    {
        if (state.CreatedAgencyId is not int id)
            return false;

        if (state.CreatedShort == null ||
            state.CreatedName == null ||
            state.CreatedType == null ||
            state.CreatedOwned == null ||
            state.CreatedActive == null)
            return false;

        var existing = Rows.FirstOrDefault(r => r.AgencyId == id);
        if (existing != null)
        {
            Selected = existing;
            return true;
        }

        var shouldBeIncluded = MatchesFilters(
            shortCode: state.CreatedShort,
            name: state.CreatedName,
            active: state.CreatedActive.Value);

        if (!shouldBeIncluded)
            return true;

        var item = new AgencyListItem
        {
            AgencyId = id,
            DispatchCenterId = state.CreatedDispatchCenterId ?? 0,
            Short = state.CreatedShort,
            Name = state.CreatedName,
            Type = state.CreatedType,
            Owned = state.CreatedOwned.Value,
            Active = state.CreatedActive.Value,
            DispatchCenterCode = state.CreatedDispatchCenterCode ?? string.Empty
        };

        var row = new AgencyRowViewModel(item, new AsyncRelayCommand(() => NavigateDetails(id)));

        var insertIndex = 0;
        for (; insertIndex < Rows.Count; insertIndex++)
        {
            if (string.Compare(Rows[insertIndex].Short, row.Short, StringComparison.OrdinalIgnoreCase) > 0)
                break;
        }

        Rows.Insert(insertIndex, row);
        Selected = row;
        return true;
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

        if (shouldBeIncluded)
        {
            _pendingSelectAgencyId = id;
            return false;
        }

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

    private Task RefreshAsync() => RefreshAsync(force: false);

    private async Task RefreshAsync(bool force)
    {
        var queryKey = $"{(int)Scope}|{SearchText}|{ActiveOnly}";
        if (!force && queryKey == _lastQueryKey && _pendingSelectAgencyId == null)
            return;

        _lastQueryKey = queryKey;

        var cts = new CancellationTokenSource();
        var prior = Interlocked.Exchange(ref _refreshCts, cts);
        prior?.Cancel();
        prior?.Dispose();

        var token = cts.Token;

        try
        {
            var items = await _query.GetAgenciesAsync(
                scope: Scope,
                searchText: SearchText,
                activeOnly: ActiveOnly,
                cancellationToken: token);

            if (token.IsCancellationRequested || !ReferenceEquals(_refreshCts, cts))
                return;

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
        catch (OperationCanceledException)
        {
            // Expected when a newer refresh starts.
        }
        finally
        {
            if (ReferenceEquals(_refreshCts, cts))
            {
                _refreshCts = null;
            }

            cts.Dispose();
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

    private Task NavigateCreateAsync()
    {
        var returnState = CaptureReturnState(Selected?.AgencyId);
        var vm = _createFactory.Create();
        _navigation.Navigate(vm, returnState);
        return Task.CompletedTask;
    }
}