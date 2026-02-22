using System.Windows;
using DucommForge.ViewModels;

namespace DucommForge;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}