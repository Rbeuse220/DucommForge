using System.Windows;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Common;
using DucommForge.ViewModels.Agencies;

namespace DucommForge.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public MainWindowViewModel(INavigationService navigation)
    {
        _navigation = navigation;

        ExitCommand = new RelayCommand(Exit);
        NavigateAgenciesCommand = new RelayCommand(NavigateAgencies);

        // Default screen
        NavigateAgencies();
    }

    public RelayCommand ExitCommand { get; }
    public RelayCommand NavigateAgenciesCommand { get; }

    public ViewModelBase? CurrentView => _navigation.Current;

    private void Exit()
    {
        Application.Current.Shutdown();
    }

    private void NavigateAgencies()
    {
        _navigation.Navigate(new AgenciesViewModel());
        Raise(nameof(CurrentView));
    }
}