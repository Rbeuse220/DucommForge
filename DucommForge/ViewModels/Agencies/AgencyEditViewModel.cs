using System.Threading.Tasks;
using DucommForge.Data;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyEditViewModel : ViewModelBase, INavigationAware
{
    private readonly AgencyDetailQueryService _query;
    private readonly AgencyCommandService _commands;
    private readonly INavigationService _navigation;

    public int AgencyId { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand CancelCommand { get; }

    public AgencyEditViewModel(
        AgencyDetailQueryService query,
        AgencyCommandService commands,
        INavigationService navigation,
        int agencyId)
    {
        _query = query;
        _commands = commands;
        _navigation = navigation;

        AgencyId = agencyId;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        CancelCommand = new AsyncRelayCommand(() => _navigation.GoBack());
    }

    public void OnNavigatedTo(NavigationState? state)
    {
        _ = LoadAsync();
    }

    private string _title = "Edit Agency";
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

    private string _short = string.Empty;
    public string Short
    {
        get => _short;
        set => SetProperty(ref _short, value);
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

    private bool CanSave()
    {
        if (IsBusy) return false;
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (string.IsNullOrWhiteSpace(Type)) return false;
        return true;
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var item = await _query.GetAgencyAsync(AgencyId);

            if (item == null)
            {
                Title = $"Edit Agency (Not Found: {AgencyId})";
                ErrorMessage = "Agency not found.";
                return;
            }

            Short = item.Short;
            Name = item.Name;
            Type = item.Type;
            Owned = item.Owned;
            Active = item.Active;
            DispatchCenterCode = item.DispatchCenterCode;
            DispatchCenterName = item.DispatchCenterName;

            Title = $"Edit Agency: {Short}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        var name = (Name ?? string.Empty).Trim();
        var type = (Type ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type))
        {
            ErrorMessage = "Name and Type are required.";
            SaveCommand.RaiseCanExecuteChanged();
            return;
        }

        IsBusy = true;

        try
        {
            var ok = await _commands.UpdateAgencyAsync(
                agencyId: AgencyId,
                name: name,
                type: type,
                owned: Owned,
                active: Active);

            if (!ok)
            {
                ErrorMessage = "Agency not found.";
                return;
            }

            _navigation.GoBack(new NavigationState
            {
                EditedAgencyId = AgencyId,
                EditedName = name,
                EditedType = type,
                EditedOwned = Owned,
                EditedActive = Active,
                SelectedAgencyId = AgencyId
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}