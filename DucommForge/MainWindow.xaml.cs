using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DucommForge.Data;
using Microsoft.EntityFrameworkCore;

namespace DucommForge;

public partial class MainWindow : Window
{
    private ObservableCollection<Station> _stations = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadConfig();
        LoadStations();
    }

    // --------------------
    // Config
    // --------------------
    private void LoadConfig()
    {
        using var db = new ForgeDbContext();
        var setting = db.AppSettings.SingleOrDefault(x => x.Key == "GrafanaBaseUrl");
        GrafanaUrlTextBox.Text = setting?.Value ?? "";
        ConfigStatusText.Text = "";
    }

    private void SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        var url = (GrafanaUrlTextBox.Text ?? "").Trim();

        using var db = new ForgeDbContext();
        var setting = db.AppSettings.SingleOrDefault(x => x.Key == "GrafanaBaseUrl");

        if (setting == null)
        {
            setting = new AppSetting { Key = "GrafanaBaseUrl", Value = url };
            db.AppSettings.Add(setting);
        }
        else
        {
            setting.Value = url;
        }

        db.SaveChanges();
        ConfigStatusText.Text = "Saved.";
    }

    // --------------------
    // Stations
    // --------------------
    private void LoadStations()
    {
        using var db = new ForgeDbContext();
        var all = db.Stations.AsNoTracking()
            .OrderBy(s => s.StationId)
            .ToList();

        _stations = new ObservableCollection<Station>(all);
        StationsGrid.ItemsSource = _stations;
        StationsStatusText.Text = "";
    }

    private void StationSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var q = (StationSearchTextBox.Text ?? "").Trim().ToUpperInvariant();

        using var db = new ForgeDbContext();
        var query = db.Stations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(s =>
                s.StationId.ToUpper().Contains(q) ||
                s.AgencyShort.ToUpper().Contains(q));
        }

        var results = query.OrderBy(s => s.StationId).ToList();
        _stations = new ObservableCollection<Station>(results);
        StationsGrid.ItemsSource = _stations;
        StationsStatusText.Text = "";
    }

    private void AddStation_Click(object sender, RoutedEventArgs e)
    {
        _stations.Add(new Station { StationId = "", AgencyShort = "", Esz = null, Active = true });
        StationsStatusText.Text = "Added row. Enter StationId and AgencyShort, then Save Changes.";
    }

    private void DeleteStation_Click(object sender, RoutedEventArgs e)
    {
        var selected = StationsGrid.SelectedItems.Cast<Station>().ToList();
        if (selected.Count == 0)
        {
            StationsStatusText.Text = "Select one or more rows to delete.";
            return;
        }

        using var db = new ForgeDbContext();

        foreach (var s in selected)
        {
            var id = (s.StationId ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(id)) continue;

            var existing = db.Stations.SingleOrDefault(x => x.StationId == id);
            if (existing != null) db.Stations.Remove(existing);
        }

        db.SaveChanges();
        LoadStations();
        StationsStatusText.Text = "Deleted.";
    }

    private void SaveStations_Click(object sender, RoutedEventArgs e)
    {
        // Validate required fields for all visible rows
        foreach (var s in _stations)
        {
            if (string.IsNullOrWhiteSpace(s.StationId) || string.IsNullOrWhiteSpace(s.AgencyShort))
            {
                StationsStatusText.Text = "Error: StationId and AgencyShort are required on all rows.";
                return;
            }
        }

        using var db = new ForgeDbContext();

        // Upsert visible rows (search filters mean you might not be seeing the whole dataset)
        foreach (var s in _stations)
        {
            var id = s.StationId.Trim().ToUpperInvariant();
            var agency = s.AgencyShort.Trim().ToUpperInvariant();
            var esz = string.IsNullOrWhiteSpace(s.Esz) ? null : s.Esz.Trim();

            var existing = db.Stations.SingleOrDefault(x => x.StationId == id);
            if (existing == null)
            {
                db.Stations.Add(new Station
                {
                    StationId = id,
                    AgencyShort = agency,
                    Esz = esz,
                    Active = s.Active
                });
            }
            else
            {
                existing.AgencyShort = agency;
                existing.Esz = esz;
                existing.Active = s.Active;
            }
        }

        db.SaveChanges();
        LoadStations();
        StationsStatusText.Text = "Saved changes.";
    }
}