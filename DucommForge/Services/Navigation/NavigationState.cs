namespace DucommForge.Services.Navigation;

public sealed class NavigationState
{
    // Agencies list return state
    // 0 = CurrentDispatchCenter, 1 = AllDispatchCenters (matches AgencyScope enum)
    public int? AgencyScope { get; init; }

    // Optional override for list scope filtering. Null or empty means use "current" center (or ALL if AgencyScope=All)
    public string? DispatchCenterScopeCode { get; init; }

    public string? SearchText { get; init; }
    public bool? ActiveOnly { get; init; }

    // Store the PK of Agency row to re-select after refresh
    public int? SelectedAgencyId { get; init; }
}