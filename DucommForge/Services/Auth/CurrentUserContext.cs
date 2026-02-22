using System.Collections.Generic;

namespace DucommForge.Services.Auth;

public sealed class CurrentUserContext
{
    public required string Username { get; init; }
    public required UserRole Role { get; init; }

    public HashSet<int> EditableDispatchCenterIds { get; init; } = new();
    public bool CanEditAllDispatchCenters { get; init; }
}