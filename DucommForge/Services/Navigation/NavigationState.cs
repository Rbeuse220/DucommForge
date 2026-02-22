namespace DucommForge.Services.Navigation;

public sealed class NavigationState
{
    // Agencies list return state
    // 0 = CurrentDispatchCenter, 1 = AllDispatchCenters (matches AgencyScope enum)
    public int? AgencyScope { get; init; }

    public string? SearchText { get; init; }
    public bool? ActiveOnly { get; init; }

    // Store the PK of Agency row to re-select after refresh
    public int? SelectedAgencyId { get; init; }

    // Post-edit optimization payload (avoids full list requery when possible)
    public int? EditedAgencyId { get; init; }
    public string? EditedName { get; init; }
    public string? EditedType { get; init; }
    public bool? EditedOwned { get; init; }
    public bool? EditedActive { get; init; }
}