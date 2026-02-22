using DucommForge.Data;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyDetailViewModel : ViewModelBase
{
    private readonly AgencyDetailQueryService _query;
    private readonly INavigationService _navigation;

    public int AgencyId { get; }

    public RelayCommand BackCommand { get; }

    public AgencyDetailViewModel(
        AgencyDetailQueryService query,
        INavigationService navigation,
        int agencyId)
    {
        _query = query;
        _navigation = navigation;
        AgencyId = agencyId;

        BackCommand = new RelayCommand(() => _navigation.GoBack());

        _ = LoadAsync();
    }

    private string _title = "Agency Details";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
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

    private async Task LoadAsync()
    {
        var item = await _query.GetAgencyAsync(AgencyId);

        if (item == null)
        {
            Title = $"Agency Details (Not Found: {AgencyId})";
            return;
        }

        Short = item.Short;
        Name = item.Name;
        Type = item.Type;
        Owned = item.Owned;
        Active = item.Active;
        DispatchCenterCode = item.DispatchCenterCode;
        DispatchCenterName = item.DispatchCenterName;

        Title = $"Agency Details: {Short}";
    }
}