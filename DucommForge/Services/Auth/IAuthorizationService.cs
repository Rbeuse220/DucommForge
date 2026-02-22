namespace DucommForge.Services.Auth;

public interface IAuthorizationService
{
    bool CanEditAgency(int dispatchCenterId);
}