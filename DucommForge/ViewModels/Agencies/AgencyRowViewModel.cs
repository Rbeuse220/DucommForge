using DucommForge.Data;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyRowViewModel : ViewModelBase
{
    public int AgencyId { get; }
    public int DispatchCenterId { get; }

    public string Short { get; }

    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _type;
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

    public string DispatchCenterCode { get; }

    public AsyncRelayCommand DetailsCommand { get; }

    public AgencyRowViewModel(
        AgencyListItem item,
        AsyncRelayCommand detailsCommand)
    {
        AgencyId = item.AgencyId;
        DispatchCenterId = item.DispatchCenterId;

        Short = item.Short;
        _name = item.Name;
        _type = item.Type;
        _owned = item.Owned;
        _active = item.Active;

        DispatchCenterCode = item.DispatchCenterCode;

        DetailsCommand = detailsCommand;
    }

    public void ApplyEdits(string name, string type, bool owned, bool active)
    {
        Name = name;
        Type = type;
        Owned = owned;
        Active = active;
    }
}