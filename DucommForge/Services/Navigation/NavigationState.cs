namespace DucommForge.Services.Navigation;

public sealed class NavigationState
{
    public string? DispatchCenterScopeCode { get; init; } // null/empty means ALL
    public string? SearchText { get; init; }
    public bool? ActiveOnly { get; init; }
    public int? SelectedAgencyId { get; init; }
}