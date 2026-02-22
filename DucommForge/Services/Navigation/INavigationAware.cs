namespace DucommForge.Services.Navigation;

public interface INavigationAware
{
    void OnNavigatedTo(NavigationState? state);
}