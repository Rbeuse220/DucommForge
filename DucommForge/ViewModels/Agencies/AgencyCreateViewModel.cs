using System;
using System.Threading;
using System.Threading.Tasks;
using DucommForge.Data;
using DucommForge.Services.Auth;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyCreateViewModel : ViewModelBase, INavigationAware
{
    private readonly CurrentDispatchCenterService _currentCenter;
    private readonly AgencyCommandService _commands;
    private readonly INavigationService _navigation;
    private readonly IAuthorizationService _auth;

    private CancellationTokenSource? _loadCts;

    private int _dispatchCenterId;

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand CancelCommand { get; }

    public AgencyCreateViewModel(
        CurrentDispatchCenterService currentCenter,
        AgencyCommandService commands,
        INavigationService navigation,
        IAuthorizationService auth)
    {
        _currentCenter = currentCenter;
        _commands = commands;
        _navigation = navigation;
        _auth = auth;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        CancelCommand = new AsyncRelayCommand(() => _navigation.GoBack());
    }

    public void OnNavigatedTo(NavigationState? state)
    {
        StartLoad();
    }

    private void StartLoad()
    {
        var cts = new CancellationTokenSource();
        var prior = Interlocked.Exchange(ref _loadCts, cts);
        prior?.Cancel();
        prior?.Dispose();

        _ = LoadAsync(cts.Token);
    }

    private string _title = "Create Agency";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (!SetProperty(ref _isBusy, value)) return;
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private bool _canCreate;
    public bool CanCreate
    {
        get => _canCreate;
        set
        {
            if (!SetProperty(ref _canCreate, value)) return;
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private string _dispatchCenterCode = string.Empty;
    public string DispatchCenterCode
    {
        get => _dispatchCenterCode;
        set => SetProperty(ref _dispatchCenterCode, value);
    }

    private string _dispatchCenterName = string.Empty;
    public string DispatchCenterName
    {
        get => _dispatchCenterName;
        set => SetProperty(ref _dispatchCenterName, value);
    }

    private string _short = string.Empty;
    public string Short
    {
        get => _short;
        set
        {
            if (!SetProperty(ref _short, value)) return;
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (!SetProperty(ref _name, value)) return;
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private string _type = string.Empty;
    public string Type
    {
        get => _type;
        set
        {
            if (!SetProperty(ref _type, value)) return;
            SaveCommand.RaiseCanExecuteChanged();
        }
    }

    private bool _owned;
    public bool Owned
    {
        get => _owned;
        set => SetProperty(ref _owned, value);
    }

    private bool _active = true;
    public bool Active
    {
        get => _active;
        set => SetProperty(ref _active, value);
    }

    private bool CanSave()
    {
        if (IsBusy) return false;
        if (!CanCreate) return false;
        if (string.IsNullOrWhiteSpace(Short)) return false;
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (string.IsNullOrWhiteSpace(Type)) return false;
        return true;
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var center = await _currentCenter.GetCurrentAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            if (center == null)
            {
                Title = "Create Agency (No Current Dispatch Center)";
                ErrorMessage = "Current dispatch center is not configured.";
                CanCreate = false;
                return;
            }

            _dispatchCenterId = center.DispatchCenterId;
            DispatchCenterCode = center.Code;
            DispatchCenterName = center.Name;

            CanCreate = _auth.CanCreateAgency(_dispatchCenterId);
            if (!CanCreate)
            {
                ErrorMessage = "You do not have permission to create agencies for this dispatch center.";
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when navigating quickly.
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            CanCreate = false;
        }
        finally
        {
            IsBusy = false;

            var cts = Interlocked.Exchange(ref _loadCts, null);
            cts?.Dispose();
        }
    }

    private static NavigationState MergeReturnStateWithCreatePayload(NavigationState? listState, NavigationState createPayload)
    {
        return new NavigationState
        {
            AgencyScope = listState?.AgencyScope,
            SearchText = listState?.SearchText,
            ActiveOnly = listState?.ActiveOnly,
            SelectedAgencyId = createPayload.CreatedAgencyId,

            CreatedAgencyId = createPayload.CreatedAgencyId,
            CreatedDispatchCenterId = createPayload.CreatedDispatchCenterId,
            CreatedDispatchCenterCode = createPayload.CreatedDispatchCenterCode,
            CreatedShort = createPayload.CreatedShort,
            CreatedName = createPayload.CreatedName,
            CreatedType = createPayload.CreatedType,
            CreatedOwned = createPayload.CreatedOwned,
            CreatedActive = createPayload.CreatedActive
        };
    }

    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        var shortCode = (Short ?? string.Empty).Trim();
        var name = (Name ?? string.Empty).Trim();
        var type = (Type ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(shortCode) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type))
        {
            ErrorMessage = "Short, Name, and Type are required.";
            SaveCommand.RaiseCanExecuteChanged();
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _commands.CreateAgencyAsync(
                dispatchCenterId: _dispatchCenterId,
                shortCode: shortCode,
                name: name,
                type: type,
                owned: Owned,
                active: Active);

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "Create failed.";
                return;
            }

            var payload = new NavigationState
            {
                CreatedAgencyId = result.AgencyId,
                CreatedDispatchCenterId = _dispatchCenterId,
                CreatedDispatchCenterCode = DispatchCenterCode,
                CreatedShort = shortCode.Trim().ToUpperInvariant(),
                CreatedName = name,
                CreatedType = type,
                CreatedOwned = Owned,
                CreatedActive = Active
            };

            var merged = MergeReturnStateWithCreatePayload(_navigation.CurrentReturnState, payload);
            _navigation.GoBack(merged);
        }
        finally
        {
            IsBusy = false;
        }
    }
}