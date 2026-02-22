using System.Collections.Generic;
using DucommForge.ViewModels.Common;

namespace DucommForge.Services.Navigation;

public sealed class NavigationService : ViewModelBase, INavigationService
{
    private readonly Stack<(ViewModelBase vm, NavigationState? state)> _stack = new();

    private ViewModelBase? _current;
    public ViewModelBase? Current
    {
        get => _current;
        private set => SetProperty(ref _current, value);
    }

    public NavigationState? CurrentReturnState =>
        _stack.Count > 0 ? _stack.Peek().state : null;

    public void Navigate(ViewModelBase viewModel, NavigationState? returnState = null)
    {
        if (Current != null)
        {
            _stack.Push((Current, returnState));
        }

        Current = viewModel;

        if (viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(null);
        }
    }

    public void GoBack()
    {
        GoBack(null);
    }

    public void GoBack(NavigationState? stateOverride)
    {
        if (_stack.Count == 0)
            return;

        var (vm, storedState) = _stack.Pop();
        var stateToUse = stateOverride ?? storedState;

        Current = vm;

        if (vm is INavigationAware aware)
        {
            aware.OnNavigatedTo(stateToUse);
        }
    }
}