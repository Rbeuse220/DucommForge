using DucommForge.Data;
using DucommForge.ViewModels.Common;

namespace DucommForge.ViewModels.Agencies;

public sealed class AgencyRowViewModel : ViewModelBase
{
    public int AgencyId { get; }
    public int DispatchCenterId { get; }

    public string Short { get; }
    public string Name { get; }
    public string Type { get; }

    public bool Owned { get; }
    public bool Active { get; }

    public string DispatchCenterCode { get; }

    public AsyncRelayCommand DetailsCommand { get; }
    public AsyncRelayCommand EditCommand { get; }

    public bool CanEdit { get; }

    public AgencyRowViewModel(
        AgencyListItem item,
        bool canEdit,
        AsyncRelayCommand detailsCommand,
        AsyncRelayCommand editCommand)
    {
        AgencyId = item.AgencyId;
        DispatchCenterId = item.DispatchCenterId;

        Short = item.Short;
        Name = item.Name;
        Type = item.Type;
        Owned = item.Owned;
        Active = item.Active;
        DispatchCenterCode = item.DispatchCenterCode;

        CanEdit = canEdit;
        DetailsCommand = detailsCommand;
        EditCommand = editCommand;
    }
}