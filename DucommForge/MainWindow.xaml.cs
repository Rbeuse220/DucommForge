using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DucommForge.Data;
using Microsoft.EntityFrameworkCore;

namespace DucommForge;

public partial class MainWindow : Window
{
    private ObservableCollection<Agency> _agencies = new();
    private ObservableCollection<Station> _stations = new();
    private ObservableCollection<string> _agencyShorts = new();
    private ObservableCollection<Unit> _units = new();
    private string? _selectedStationId = null;

    public MainWindow()
    {
        InitializeComponent();
        LoadConfig();
        LoadAgencies();   // also binds agency dropdown in Stations grid
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
    // Agencies
    // --------------------
    private void LoadAgencies()
    {
        using var db = new ForgeDbContext();
        var all = db.Agencies.AsNoTracking()
            .OrderBy(a => a.Short)
            .ToList();

        _agencies = new ObservableCollection<Agency>(all);
        AgenciesGrid.ItemsSource = _agencies;
        AgenciesStatusText.Text = "";

        _agencyShorts = new ObservableCollection<string>(
            _agencies.Select(a => a.Short).OrderBy(x => x).ToList()
        );

        // Bind Stations grid dropdown (column index 1 = Agency column)
        if (StationsGrid.Columns.Count > 1 && StationsGrid.Columns[1] is System.Windows.Controls.DataGridComboBoxColumn c)
        {
            c.ItemsSource = _agencyShorts;
        }
    }

    private void AgencySearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var q = (AgencySearchTextBox.Text ?? "").Trim().ToUpperInvariant();

        using var db = new ForgeDbContext();
        var query = db.Agencies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(a =>
                a.Short.ToUpper().Contains(q) ||
                (a.Name ?? "").ToUpper().Contains(q));
        }

        var results = query.OrderBy(a => a.Short).ToList();
        _agencies = new ObservableCollection<Agency>(results);
        AgenciesGrid.ItemsSource = _agencies;
        AgenciesStatusText.Text = "";
    }

    private void AddAgency_Click(object sender, RoutedEventArgs e)
    {
        _agencies.Add(new Agency { Short = "", Name = null, Type = "fire", Owned = true, Active = true });
        AgenciesStatusText.Text = "Added row. Enter Short, then Save Changes.";
    }

    private void DeleteAgency_Click(object sender, RoutedEventArgs e)
    {
        var selected = AgenciesGrid.SelectedItems.Cast<Agency>().ToList();
        if (selected.Count == 0)
        {
            AgenciesStatusText.Text = "Select one or more rows to delete.";
            return;
        }

        using var db = new ForgeDbContext();

        foreach (var a in selected)
        {
            var key = (a.Short ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(key)) continue;

            var existing = db.Agencies.SingleOrDefault(x => x.Short == key);
            if (existing != null) db.Agencies.Remove(existing);
        }

        try
        {
            db.SaveChanges();
            LoadAgencies();
            AgenciesStatusText.Text = "Deleted.";
        }
        catch (DbUpdateException)
        {
            AgenciesStatusText.Text = "Delete failed. This agency is referenced by stations.";
        }
    }

    private void SaveAgencies_Click(object sender, RoutedEventArgs e)
    {
        foreach (var a in _agencies)
        {
            if (string.IsNullOrWhiteSpace(a.Short))
            {
                AgenciesStatusText.Text = "Error: Agency Short is required on all rows.";
                return;
            }
            if (string.IsNullOrWhiteSpace(a.Type))
            {
                a.Type = "fire";
            }
        }

        using var db = new ForgeDbContext();

        var ducommId = db.DispatchCenters.Single(x => x.Code == "DUCOMM").DispatchCenterId;

        foreach (var a in _agencies)
        {
            var key = a.Short.Trim().ToUpperInvariant();
            var name = string.IsNullOrWhiteSpace(a.Name) ? null : a.Name.Trim();
            var type = a.Type.Trim().ToLowerInvariant();

            var existing = db.Agencies.SingleOrDefault(x => x.Short == key);
            if (existing == null)
            {
                db.Agencies.Add(new Agency
                {
                    Short = key,
                    DispatchCenterId = ducommId,
                    Name = name,
                    Type = type,
                    Owned = a.Owned,
                    Active = a.Active
                });
            }
            else
            {
                existing.Name = name;
                existing.Type = type;
                existing.Owned = a.Owned;
                existing.Active = a.Active;
            }
        }

        db.SaveChanges();
        LoadAgencies(); // refresh dropdown list too
        AgenciesStatusText.Text = "Saved changes.";
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
        var defaultAgency = _agencyShorts.FirstOrDefault() ?? "";
        _stations.Add(new Station { StationId = "", AgencyShort = defaultAgency, Esz = null, Active = true });
        StationsStatusText.Text = "Added row. Enter StationId, then Save Changes.";
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
        foreach (var s in _stations)
        {
            if (string.IsNullOrWhiteSpace(s.StationId) || string.IsNullOrWhiteSpace(s.AgencyShort))
            {
                StationsStatusText.Text = "Error: StationId and Agency are required on all rows.";
                return;
            }
        }

        using var db = new ForgeDbContext();

        var validAgencies = db.Agencies.AsNoTracking()
            .Select(a => a.Short)
            .ToHashSet();

        foreach (var s in _stations)
        {
            var id = s.StationId.Trim().ToUpperInvariant();
            var agency = s.AgencyShort.Trim().ToUpperInvariant();
            var esz = string.IsNullOrWhiteSpace(s.Esz) ? null : s.Esz.Trim();

            if (!validAgencies.Contains(agency))
            {
                StationsStatusText.Text = $"Error: Agency '{agency}' does not exist. Create it on the Agencies tab first.";
                return;
            }

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

    // --------------------
    // Units
    // --------------------
    private void StationsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Only support single-station detail view
        if (StationsGrid.SelectedItems.Count != 1)
        {
            _selectedStationId = null;
            _units = new ObservableCollection<Unit>();
            UnitsGrid.ItemsSource = _units;
            UnitsStatusText.Text = "Select exactly one station to view/edit units.";
            return;
        }

        var station = StationsGrid.SelectedItem as Station;
        _selectedStationId = station?.StationId?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(_selectedStationId))
        {
            _units = new ObservableCollection<Unit>();
            UnitsGrid.ItemsSource = _units;
            UnitsStatusText.Text = "Selected station has no StationId yet. Save the station first.";
            return;
        }

        LoadUnitsForStation(_selectedStationId);
    }

    private void LoadUnitsForStation(string stationId)
    {
        using var db = new ForgeDbContext();
        var all = db.Units.AsNoTracking()
            .Where(u => u.StationId == stationId)
            .OrderBy(u => u.UnitId)
            .ToList();

        _units = new ObservableCollection<Unit>(all);
        UnitsGrid.ItemsSource = _units;
        UnitsStatusText.Text = "";
    }

    private void AddUnit_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedStationId))
        {
            UnitsStatusText.Text = "Select exactly one saved station first.";
            return;
        }

        _units.Add(new Unit
        {
            UnitId = "",
            StationId = _selectedStationId,
            Type = "",
            Jump = false,
            Active = true
        });

        UnitsStatusText.Text = "Added row. Enter Unit and Type, then Save Units.";
    }

    private void DeleteUnit_Click(object sender, RoutedEventArgs e)
    {
        var selected = UnitsGrid.SelectedItems.Cast<Unit>().ToList();
        if (selected.Count == 0)
        {
            UnitsStatusText.Text = "Select one or more units to delete.";
            return;
        }

        using var db = new ForgeDbContext();

        foreach (var u in selected)
        {
            var unitId = (u.UnitId ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(unitId)) continue;

            var existing = db.Units.SingleOrDefault(x => x.UnitId == unitId);
            if (existing != null) db.Units.Remove(existing);
        }

        db.SaveChanges();

        if (!string.IsNullOrWhiteSpace(_selectedStationId))
            LoadUnitsForStation(_selectedStationId);

        UnitsStatusText.Text = "Deleted.";
    }

    private void SaveUnits_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedStationId))
        {
            UnitsStatusText.Text = "Select exactly one station first.";
            return;
        }

        // Validate required fields
        foreach (var u in _units)
        {
            if (string.IsNullOrWhiteSpace(u.UnitId) || string.IsNullOrWhiteSpace(u.Type))
            {
                UnitsStatusText.Text = "Error: Unit and Type are required on all rows.";
                return;
            }
        }

        using var db = new ForgeDbContext();

        // Ensure station exists (FK safety)
        var stationExists = db.Stations.AsNoTracking().Any(s => s.StationId == _selectedStationId);
        if (!stationExists)
        {
            UnitsStatusText.Text = "Error: Selected station does not exist in DB. Save the station first.";
            return;
        }

        foreach (var u in _units)
        {
            var unitId = u.UnitId.Trim().ToUpperInvariant();
            var type = u.Type.Trim();
            var stationId = _selectedStationId;
            var existing = db.Units.SingleOrDefault(x => x.UnitId == unitId);

            if (existing == null)
            {
                db.Units.Add(new Unit
                {
                    UnitId = unitId,
                    StationId = stationId,
                    Type = type,
                    Jump = u.Jump,
                    Active = u.Active
                });
            }
            else
            {
                existing.StationId = stationId;
                existing.Type = type;
                existing.Jump = u.Jump;
                existing.Active = u.Active;
            }
        }

        db.SaveChanges();
        LoadUnitsForStation(_selectedStationId);
        UnitsStatusText.Text = "Saved units.";
    }
}