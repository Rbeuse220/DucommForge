using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DucommForge.ViewModels.Common;

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    // Allows: new AsyncRelayCommand(() => DoSomethingVoid());
    public AsyncRelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException(nameof(execute));
        _execute = () =>
        {
            execute();
            return Task.CompletedTask;
        };
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    // Optional hook so async failures are observed and can be handled/logged by the caller.
    public event EventHandler<Exception>? ExecutionFailed;

    public bool CanExecute(object? parameter)
    {
        if (_isExecuting) return false;
        return _canExecute?.Invoke() ?? true;
    }

    // ICommand.Execute must be void. Do not use async void here.
    public void Execute(object? parameter)
    {
        _ = ExecuteAsync();
    }

    public async Task ExecuteAsync()
    {
        if (!CanExecute(null)) return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            ExecutionFailed?.Invoke(this, ex);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}