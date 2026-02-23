namespace DucommForge.Services.Auth;

public sealed class AuthorizationService(CurrentUserContext user) : IAuthorizationService
{
    public bool CanCreateAgency(int dispatchCenterId)
    {
        if (user.Role == UserRole.SuperAdmin) return true;
        if (user.Role == UserRole.Admin) return true;

        if (user.Role == UserRole.Editor)
        {
            return user.CanEditAllDispatchCenters || user.EditableDispatchCenterIds.Contains(dispatchCenterId);
        }

        return false;
    }

    public bool CanEditAgency(int dispatchCenterId)
    {
        if (user.Role == UserRole.SuperAdmin) return true;
        if (user.Role == UserRole.Admin) return true;

        if (user.Role == UserRole.Editor)
        {
            return user.CanEditAllDispatchCenters || user.EditableDispatchCenterIds.Contains(dispatchCenterId);
        }

        return false;
    }
}