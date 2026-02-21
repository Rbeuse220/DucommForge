using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DucommForge.Data;
using Microsoft.EntityFrameworkCore;

namespace DucommForge;

public partial class MainWindow : Window
{
    private ObservableCollection<DispatchCenter> _dispatchCenters = new();
    private ObservableCollection<Agency> _agencies = new();
    private ObservableCollection<Station> _stations = new();
    private ObservableCollection<string> _agencyShorts = new();
    private ObservableCollection<Unit> _units = new();

    private string? _currentDispatchCenterCode = null;
    private int? _currentDispatchCenterId = null;

    private string? _selectedStationId = null;

    public MainWindow()
    {
        InitializeComponent();

        UnitsGrid.ItemsSource = _units;
        SetUnitsUiEnabled(false);
        UnitsStatusText.Text = "Select exactly one station to view/edit units.";

        LoadConfig();
        LoadDispatchCenters();
        ApplyDispatchCenterScopeFromSetting();
        SetScopeUi();

        LoadAgencies();
        LoadStations();
    }

    // --------------------
    // Config
    // --------------------
    private void LoadConfig()
    {
        using var db = new ForgeDbContext();

        var grafana = db.AppSettings.SingleOrDefault(x => x.Key == "GrafanaBaseUrl");
        GrafanaUrlTextBox.Text = grafana?.Value ?? "";

        var currentDc = db.AppSettings.SingleOrDefault(x => x.Key == "CurrentDispatchCenterCode");
        _currentDispatchCenterCode = string.IsNullOrWhiteSpace(currentDc?.Value) ? null : currentDc!.Value.Trim().ToUpperInvariant();

        ConfigStatusText.Text = "";
    }

    private void SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        var url = (GrafanaUrlTextBox.Text ?? "").Trim();
        var selectedCode = (CurrentDispatchCenterComboBox.SelectedItem as string ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(selectedCode)) selectedCode = "";

        using var db = new ForgeDbContext();

        UpsertAppSetting(db, "GrafanaBaseUrl", url);
        UpsertAppSetting(db, "CurrentDispatchCenterCode", selectedCode);

        db.SaveChanges();

        _currentDispatchCenterCode = string.IsNullOrWhiteSpace(selectedCode) ? null : selectedCode;

        ApplyDispatchCenterScopeFromSetting();
        SetScopeUi();
        LoadAgencies();
        LoadStations();

        ConfigStatusText.Text = "Saved.";
    }

    private void CurrentDispatchCenterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (CurrentDispatchCenterComboBox.ItemsSource == null)
            return;

        var selectedCode = (CurrentDispatchCenterComboBox.SelectedItem as string ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(selectedCode))
            return;

        using var db = new ForgeDbContext();
        UpsertAppSetting(db, "CurrentDispatchCenterCode", selectedCode);
        db.SaveChanges();

        _currentDispatchCenterCode = selectedCode;

        ApplyDispatchCenterScopeFromSetting();
        SetScopeUi();
        LoadAgencies();
        LoadStations();

        ConfigStatusText.Text = $"Scope set to {selectedCode}.";
    }

    private static void UpsertAppSetting(ForgeDbContext db, string key, string value)
    {
        var setting = db.AppSettings.SingleOrDefault(x => x.Key == key);
        if (setting == null)
        {
            db.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
    }

    private void ApplyDispatchCenterScopeFromSetting()
    {
        using var db = new ForgeDbContext();

        if (string.IsNullOrWhiteSpace(_currentDispatchCenterCode))
        {
            _currentDispatchCenterId = null;
            SetDispatchCenterComboSelection(null);
            return;
        }

        var dc = db.DispatchCenters.AsNoTracking().SingleOrDefault(x => x.Code == _currentDispatchCenterCode);
        if (dc == null)
        {
            _currentDispatchCenterId = null;
            SetDispatchCenterComboSelection(null);
            ConfigStatusText.Text = $"CurrentDispatchCenterCode '{_currentDispatchCenterCode}' not found. Select one.";
            return;
        }

        _currentDispatchCenterId = dc.DispatchCenterId;
        SetDispatchCenterComboSelection(dc.Code);
    }

    private void SetDispatchCenterComboSelection(string? code)
    {
        if (CurrentDispatchCenterComboBox.ItemsSource == null)
            return;

        if (string.IsNullOrWhiteSpace(code))
        {
            CurrentDispatchCenterComboBox.SelectedIndex = -1;
            return;
        }

        var list = CurrentDispatchCenterComboBox.ItemsSource as ObservableCollection<string>;
        if (list == null) return;

        var match = list.FirstOrDefault(x => string.Equals(x, code, StringComparison.OrdinalIgnoreCase));
        CurrentDispatchCenterComboBox.SelectedItem = match;
    }

    private void SetScopeUi()
    {
        var hasScope = _currentDispatchCenterId.HasValue;

        AgenciesTab.IsEnabled = hasScope;
        StationsTab.IsEnabled = hasScope;

        var scopeLabel = hasScope ? _currentDispatchCenterCode : "NO SCOPE";
        Title = $"DucommForge [{scopeLabel}]";

        if (!hasScope)
        {
            AgenciesStatusText.Text = "Select a Current Dispatch Center in Config.";
            StationsStatusText.Text = "Select a Current Dispatch Center in Config.";
            ClearUnitsSelectionState("Select exactly one station to view/edit units.");
        }
    }

    // --------------------
    // Dispatch Centers
    // --------------------
    private void LoadDispatchCenters()
    {
        using var db = new ForgeDbContext();

        var all = db.DispatchCenters.AsNoTracking()
            .OrderBy(x => x.Code)
            .ToList();

        _dispatchCenters = new ObservableCollection<DispatchCenter>(all);
        DispatchCentersGrid.ItemsSource = _dispatchCenters;
        DispatchCentersStatusText.Text = "";

        var codes = new ObservableCollection<string>(
            _dispatchCenters.Select(x => x.Code).OrderBy(x => x).ToList()
        );
        CurrentDispatchCenterComboBox.ItemsSource = codes;

        if (!string.IsNullOrWhiteSpace(_currentDispatchCenterCode))
            SetDispatchCenterComboSelection(_currentDispatchCenterCode);
    }

    private void DispatchCenterSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var q = (DispatchCenterSearchTextBox.Text ?? "").Trim().ToUpperInvariant();

        using var db = new ForgeDbContext();
        var query = db.DispatchCenters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                x.Code.ToUpper().Contains(q) ||
                (x.Name ?? "").ToUpper().Contains(q));
        }

        var results = query.OrderBy(x => x.Code).ToList();
        _dispatchCenters = new ObservableCollection<DispatchCenter>(results);
        DispatchCentersGrid.ItemsSource = _dispatchCenters;
        DispatchCentersStatusText.Text = "";
    }

    private void AddDispatchCenter_Click(object sender, RoutedEventArgs e)
    {
        _dispatchCenters.Add(new DispatchCenter { Code = "", Name = null, Active = true });
        DispatchCentersStatusText.Text = "Added row. Enter Code, then Save Changes.";
    }

    private void DeleteDispatchCenter_Click(object sender, RoutedEventArgs e)
    {
        var selected = DispatchCentersGrid.SelectedItems.Cast<DispatchCenter>().ToList();
        if (selected.Count == 0)
        {
            DispatchCentersStatusText.Text = "Select one or more rows to delete.";
            return;
        }

        using var db = new ForgeDbContext();

        foreach (var dc in selected)
        {
            var id = dc.DispatchCenterId;
            if (id <= 0) continue;

            var existing = db.DispatchCenters.SingleOrDefault(x => x.DispatchCenterId == id);
            if (existing != null) db.DispatchCenters.Remove(existing);
        }

        try
        {
            db.SaveChanges();
            LoadDispatchCenters();
            DispatchCentersStatusText.Text = "Deleted.";

            ApplyDispatchCenterScopeFromSetting();
            SetScopeUi();
            LoadAgencies();
            LoadStations();
        }
        catch (DbUpdateException)
        {
            DispatchCentersStatusText.Text = "Delete failed. This dispatch center is referenced by agencies.";
        }
    }

    private void SaveDispatchCenters_Click(object sender, RoutedEventArgs e)
    {
        foreach (var dc in _dispatchCenters)
        {
            if (string.IsNullOrWhiteSpace(dc.Code))
            {
                DispatchCentersStatusText.Text = "Error: Dispatch Center Code is required on all rows.";
                return;
            }
        }

        using var db = new ForgeDbContext();

        foreach (var dc in _dispatchCenters)
        {
            var code = dc.Code.Trim().ToUpperInvariant();
            var name = string.IsNullOrWhiteSpace(dc.Name) ? null : dc.Name.Trim();

            var existing = db.DispatchCenters.SingleOrDefault(x => x.Code == code);
            if (existing == null)
            {
                db.DispatchCenters.Add(new DispatchCenter
                {
                    Code = code,
                    Name = name,
                    Active = dc.Active
                });
            }
            else
            {
                existing.Name = name;
                existing.Active = dc.Active;
            }
        }

        db.SaveChanges();
        LoadDispatchCenters();
        DispatchCentersStatusText.Text = "Saved changes.";

        ApplyDispatchCenterScopeFromSetting();
        SetScopeUi();
        LoadAgencies();
        LoadStations();
    }

    // --------------------
    // Agencies
    // --------------------
    private void LoadAgencies()
    {
        using var db = new ForgeDbContext();

        if (!_currentDispatchCenterId.HasValue)
        {
            _agencies = new ObservableCollection<Agency>();
            AgenciesGrid.ItemsSource = _agencies;
            AgenciesStatusText.Text = "Select a Current Dispatch Center in Config.";
            _agencyShorts = new ObservableCollection<string>();
            if (StationsGrid.Columns.Count > 1 && StationsGrid.Columns[1] is System.Windows.Controls.DataGridComboBoxColumn c0)
                c0.ItemsSource = _agencyShorts;
            return;
        }

        var all = db.Agencies.AsNoTracking()
            .Where(a => a.DispatchCenterId == _currentDispatchCenterId.Value)
            .OrderBy(a => a.Short)
            .ToList();

        _agencies = new ObservableCollection<Agency>(all);
        AgenciesGrid.ItemsSource = _agencies;
        AgenciesStatusText.Text = "";

        _agencyShorts = new ObservableCollection<string>(
            _agencies.Select(a => a.Short).OrderBy(x => x).ToList()
        );

        if (StationsGrid.Columns.Count > 1 && StationsGrid.Columns[1] is System.Windows.Controls.DataGridComboBoxColumn c)
        {
            c.ItemsSource = _agencyShorts;
        }

        ClearUnitsSelectionState("Select exactly one station to view/edit units.");
    }

    private void AgencySearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var q = (AgencySearchTextBox.Text ?? "").Trim().ToUpperInvariant();

        using var db = new ForgeDbContext();

        if (!_currentDispatchCenterId.HasValue)
        {
            _agencies = new ObservableCollection<Agency>();
            AgenciesGrid.ItemsSource = _agencies;
            AgenciesStatusText.Text = "Select a Current Dispatch Center in Config.";
            return;
        }

        var query = db.Agencies.AsNoTracking()
            .Where(a => a.DispatchCenterId == _currentDispatchCenterId.Value);

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

        _agencyShorts = new ObservableCollection<string>(
            _agencies.Select(a => a.Short).OrderBy(x => x).ToList()
        );

        if (StationsGrid.Columns.Count > 1 && StationsGrid.Columns[1] is System.Windows.Controls.DataGridComboBoxColumn c)
        {
            c.ItemsSource = _agencyShorts;
        }
    }

    private void AddAgency_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentDispatchCenterId.HasValue)
        {
            AgenciesStatusText.Text = "Select a Current Dispatch Center in Config first.";
            return;
        }

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
        if (!_currentDispatchCenterId.HasValue)
        {
            AgenciesStatusText.Text = "Select a Current Dispatch Center in Config first.";
            return;
        }

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
                    DispatchCenterId = _currentDispatchCenterId.Value,
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
        LoadAgencies();
        AgenciesStatusText.Text = "Saved changes.";
    }

    // --------------------
    // Stations
    // --------------------
    private void LoadStations()
    {
        using var db = new ForgeDbContext();

        if (!_currentDispatchCenterId.HasValue)
        {
            _stations = new ObservableCollection<Station>();
            StationsGrid.ItemsSource = _stations;
            StationsStatusText.Text = "Select a Current Dispatch Center in Config.";
            ClearUnitsSelectionState("Select exactly one station to view/edit units.");
            return;
        }

        var agencyShorts = db.Agencies.AsNoTracking()
            .Where(a => a.DispatchCenterId == _currentDispatchCenterId.Value)
            .Select(a => a.Short)
            .ToList();

        var all = db.Stations.AsNoTracking()
            .Where(s => agencyShorts.Contains(s.AgencyShort))
            .OrderBy(s => s.StationId)
            .ToList();

        _stations = new ObservableCollection<Station>(all);
        StationsGrid.ItemsSource = _stations;
        StationsStatusText.Text = "";

        ClearUnitsSelectionState("Select exactly one station to view/edit units.");
    }

    private void StationSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var q = (StationSearchTextBox.Text ?? "").Trim().ToUpperInvariant();

        using var db = new ForgeDbContext();

        if (!_currentDispatchCenterId.HasValue)
        {
            _stations = new ObservableCollection<Station>();
            StationsGrid.ItemsSource = _stations;
            StationsStatusText.Text = "Select a Current Dispatch Center in Config.";
            ClearUnitsSelectionState("Select exactly one station to view/edit units.");
            return;
        }

        var agencyShorts = db.Agencies.AsNoTracking()
            .Where(a => a.DispatchCenterId == _currentDispatchCenterId.Value)
            .Select(a => a.Short)
            .ToList();

        var query = db.Stations.AsNoTracking()
            .Where(s => agencyShorts.Contains(s.AgencyShort));

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

        ClearUnitsSelectionState("Select exactly one station to view/edit units.");
    }

    private void AddStation_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentDispatchCenterId.HasValue)
        {
            StationsStatusText.Text = "Select a Current Dispatch Center in Config first.";
            return;
        }

        var defaultAgency = _agencyShorts.FirstOrDefault() ?? "";
        _stations.Add(new Station { StationId = "", AgencyShort = defaultAgency, Esz = null, Active = true });
        StationsStatusText.Text = "Added row. Enter StationId, then Save Changes.";

        ClearUnitsSelectionState("Selected station has no StationId yet. Save the station first.");
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

        ClearUnitsSelectionState("Select exactly one station to view/edit units.");
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

        if (!_currentDispatchCenterId.HasValue)
        {
            StationsStatusText.Text = "Select a Current Dispatch Center in Config first.";
            return;
        }

        var validAgencies = db.Agencies.AsNoTracking()
            .Where(a => a.DispatchCenterId == _currentDispatchCenterId.Value)
            .Select(a => a.Short)
            .ToHashSet();

        foreach (var s in _stations)
        {
            var id = s.StationId.Trim().ToUpperInvariant();
            var agency = s.AgencyShort.Trim().ToUpperInvariant();
            var esz = string.IsNullOrWhiteSpace(s.Esz) ? null : s.Esz.Trim();

            if (!validAgencies.Contains(agency))
            {
                StationsStatusText.Text = $"Error: Agency '{agency}' does not exist in the current dispatch center scope.";
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

        ClearUnitsSelectionState("Select exactly one station to view/edit units.");
    }

    // --------------------
    // Units
    // --------------------
    private void StationsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        UnitsStatusText.Text = "";

        if (StationsGrid.SelectedItems.Count != 1)
        {
            ClearUnitsSelectionState("Select exactly one station to view/edit units.");
            return;
        }

        var station = StationsGrid.SelectedItem as Station;
        _selectedStationId = station?.StationId?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(_selectedStationId))
        {
            ClearUnitsSelectionState("Selected station has no StationId yet. Save the station first.");
            return;
        }

        try
        {
            LoadUnitsForStation(_selectedStationId);
            SetUnitsUiEnabled(true);
            UnitsStatusText.Text = $"Loaded {_units.Count} unit(s) for {_selectedStationId}.";
        }
        catch (Exception ex)
        {
            ClearUnitsSelectionState(ex.Message);
        }
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
    }

    private void AddUnit_Click(object sender, RoutedEventArgs e)
    {
        UnitsStatusText.Text = "";

        if (string.IsNullOrWhiteSpace(_selectedStationId))
        {
            UnitsStatusText.Text = "Select exactly one saved station first.";
            return;
        }

        var newUnit = new Unit
        {
            UnitId = "",
            StationId = _selectedStationId,
            Type = "",
            Jump = false,
            Active = true
        };

        _units.Add(newUnit);

        UnitsGrid.SelectedItem = newUnit;
        UnitsGrid.ScrollIntoView(newUnit);

        UnitsStatusText.Text = "Added row. Enter Unit and Type, then Save Units.";
    }

    private void DeleteUnit_Click(object sender, RoutedEventArgs e)
    {
        UnitsStatusText.Text = "";

        if (UnitsGrid.SelectedItems.Count == 0)
        {
            UnitsStatusText.Text = "Select one or more units to delete.";
            return;
        }

        var toRemove = UnitsGrid.SelectedItems.Cast<Unit>().ToList();
        foreach (var u in toRemove)
            _units.Remove(u);

        UnitsStatusText.Text = $"Removed {toRemove.Count} unit(s). Click Save Units to persist.";
    }

    private void SaveUnits_Click(object sender, RoutedEventArgs e)
    {
        UnitsStatusText.Text = "";

        if (string.IsNullOrWhiteSpace(_selectedStationId))
        {
            UnitsStatusText.Text = "Select exactly one station first.";
            return;
        }

        foreach (var u in _units)
        {
            u.UnitId = (u.UnitId ?? "").Trim().ToUpperInvariant();
            u.Type = (u.Type ?? "").Trim();
            u.StationId = _selectedStationId;

            if (string.IsNullOrWhiteSpace(u.UnitId) || string.IsNullOrWhiteSpace(u.Type))
            {
                UnitsStatusText.Text = "Error: Unit and Type are required on all rows.";
                return;
            }
        }

        var dup = _units
            .GroupBy(u => u.UnitId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);

        if (dup != null)
        {
            UnitsStatusText.Text = $"Error: Duplicate Unit in grid: '{dup.Key}'.";
            return;
        }

        using var db = new ForgeDbContext();

        var stationExists = db.Stations.AsNoTracking().Any(s => s.StationId == _selectedStationId);
        if (!stationExists)
        {
            UnitsStatusText.Text = "Error: Selected station does not exist in DB. Save the station first.";
            return;
        }

        var dbUnitsForStation = db.Units.Where(u => u.StationId == _selectedStationId).ToList();
        var desiredIds = _units.Select(u => u.UnitId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var existing in dbUnitsForStation)
        {
            if (!desiredIds.Contains(existing.UnitId))
                db.Units.Remove(existing);
        }

        foreach (var u in _units)
        {
            var existing = db.Units.SingleOrDefault(x => x.UnitId == u.UnitId);

            if (existing == null)
            {
                db.Units.Add(new Unit
                {
                    UnitId = u.UnitId,
                    StationId = _selectedStationId,
                    Type = u.Type,
                    Jump = u.Jump,
                    Active = u.Active
                });
            }
            else
            {
                existing.StationId = _selectedStationId;
                existing.Type = u.Type;
                existing.Jump = u.Jump;
                existing.Active = u.Active;
            }
        }

        db.SaveChanges();

        LoadUnitsForStation(_selectedStationId);
        UnitsStatusText.Text = "Saved units.";
    }

    private void SetUnitsUiEnabled(bool enabled)
    {
        UnitsGrid.IsEnabled = enabled;
        if (AddUnitButton != null) AddUnitButton.IsEnabled = enabled;
        if (DeleteUnitButton != null) DeleteUnitButton.IsEnabled = enabled;
        if (SaveUnitsButton != null) SaveUnitsButton.IsEnabled = enabled;
    }

    private void ClearUnitsSelectionState(string status)
    {
        _selectedStationId = null;
        _units.Clear();
        UnitsGrid.ItemsSource = _units;
        SetUnitsUiEnabled(false);
        UnitsStatusText.Text = status;
    }
}