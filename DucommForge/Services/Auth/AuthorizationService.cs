namespace DucommForge.Services.Auth;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly CurrentUserContext _user;

    public AuthorizationService(CurrentUserContext user)
    {
        _user = user;
    }

    public bool CanEditAgency(int dispatchCenterId)
    {
        if (_user.Role == UserRole.SuperAdmin) return true;
        if (_user.Role == UserRole.Admin) return true;

        if (_user.Role == UserRole.Editor)
        {
            return _user.CanEditAllDispatchCenters || _user.EditableDispatchCenterIds.Contains(dispatchCenterId);
        }

        return false;
    }
}