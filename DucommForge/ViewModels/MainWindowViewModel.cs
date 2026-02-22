using System.ComponentModel;
using System.Windows;
using DucommForge.Services.Navigation;
using DucommForge.ViewModels.Agencies;
using DucommForge.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DucommForge.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;

    public MainWindowViewModel(INavigationService navigation, IServiceProvider services)
    {
        _navigation = navigation;
        _services = services;

        ExitCommand = new RelayCommand(Exit);
        NavigateAgenciesCommand = new RelayCommand(NavigateAgencies);

        if (_navigation is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.Current))
                {
                    Raise(nameof(CurrentView));
                }
            };
        }

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
        var vm = _services.GetRequiredService<AgenciesViewModel>();
        _navigation.Navigate(vm);
        Raise(nameof(CurrentView));
    }
}