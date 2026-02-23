namespace DucommForge.Services.Auth;

public interface IAuthorizationService
{
    bool CanCreateAgency(int dispatchCenterId);
    bool CanEditAgency(int dispatchCenterId);
}