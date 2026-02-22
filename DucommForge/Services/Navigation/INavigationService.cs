using DucommForge.ViewModels.Common;

namespace DucommForge.Services.Navigation;

public interface INavigationService
{
    ViewModelBase? Current { get; }

    void Navigate(ViewModelBase viewModel, NavigationState? returnState = null);
    void GoBack();

    NavigationState? CurrentReturnState { get; }
}