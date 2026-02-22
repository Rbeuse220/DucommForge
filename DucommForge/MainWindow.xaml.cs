using System.Windows;

namespace DucommForge;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "DucommForge\n\nConfiguration editor for Dispatch Centers, Agencies, Stations, and Units.",
            "About DucommForge",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}