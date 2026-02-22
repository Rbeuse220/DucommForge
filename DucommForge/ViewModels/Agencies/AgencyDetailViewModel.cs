using System.Threading.Tasks;
using DucommForge.Data;
using DucommForge.Services.Auth;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyDetailViewModel : ViewModelBase, INavigationAware
{
    private readonly AgencyDetailQueryService _query;
    private readonly INavigationService _navigation;
    private readonly IAuthorizationService _auth;
    private readonly IAgencyEditViewModelFactory _editFactory;

    private NavigationState? _listReturnState;

    public int AgencyId { get; }

    public AsyncRelayCommand BackCommand { get; }
    public AsyncRelayCommand EditCommand { get; }

    public AgencyDetailViewModel(
        AgencyDetailQueryService query,
        INavigationService navigation,
        IAuthorizationService auth,
        IAgencyEditViewModelFactory editFactory,
        int agencyId)
    {
        _query = query;
        _navigation = navigation;
        _auth = auth;
        _editFactory = editFactory;

        AgencyId = agencyId;

        BackCommand = new AsyncRelayCommand(() => _navigation.GoBack());
        EditCommand = new AsyncRelayCommand(NavigateEditAsync, () => CanEdit);
    }

    public void OnNavigatedTo(NavigationState? state)
    {
        if (state != null)
        {
            if (state.AgencyScope != null ||
                state.SearchText != null ||
                state.ActiveOnly != null ||
                state.SelectedAgencyId != null)
            {
                _listReturnState = state;
            }

            if (state.EditedAgencyId is int editedId && editedId == AgencyId)
            {
                var merged = MergeListStateWithEditPayload(_listReturnState, state);
                _navigation.GoBack(merged);
                return;
            }
        }

        _ = LoadAsync();
    }

    private static NavigationState MergeListStateWithEditPayload(NavigationState? listState, NavigationState editState)
    {
        return new NavigationState
        {
            AgencyScope = listState?.AgencyScope,
            SearchText = listState?.SearchText,
            ActiveOnly = listState?.ActiveOnly,
            SelectedAgencyId = editState.EditedAgencyId ?? listState?.SelectedAgencyId,

            EditedAgencyId = editState.EditedAgencyId,
            EditedName = editState.EditedName,
            EditedType = editState.EditedType,
            EditedOwned = editState.EditedOwned,
            EditedActive = editState.EditedActive
        };
    }

    private string _title = "Agency Details";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private bool _canEdit;
    public bool CanEdit
    {
        get => _canEdit;
        set
        {
            if (!SetProperty(ref _canEdit, value)) return;
            EditCommand.RaiseCanExecuteChanged();
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
        set => SetProperty(ref _name, value);
    }

    private string _type = string.Empty;
    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private bool _owned;
    public bool Owned
    {
        get => _owned;
        set => SetProperty(ref _owned, value);
    }

    private bool _active;
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

    private int _dispatchCenterId;
    public int DispatchCenterId
    {
        get => _dispatchCenterId;
        set => SetProperty(ref _dispatchCenterId, value);
    }

    private async Task LoadAsync()
    {
        var item = await _query.GetAgencyAsync(AgencyId);

        if (item == null)
        {
            Title = $"Agency Details (Not Found: {AgencyId})";
            CanEdit = false;
            return;
        }

        Short = item.Short;
        Name = item.Name;
        Type = item.Type;
        Owned = item.Owned;
        Active = item.Active;
        DispatchCenterCode = item.DispatchCenterCode;
        DispatchCenterName = item.DispatchCenterName;
        DispatchCenterId = item.DispatchCenterId;

        CanEdit = _auth.CanEditAgency(item.DispatchCenterId);

        Title = $"Agency Details: {Short}";
    }

    private Task NavigateEditAsync()
    {
        _listReturnState = _navigation.CurrentReturnState;

        var vm = _editFactory.Create(AgencyId);
        _navigation.Navigate(vm, _listReturnState);

        return Task.CompletedTask;
    }
}