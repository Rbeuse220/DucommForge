using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DucommForge.Data;
using DucommForge.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DucommForge.Views.Legacy;

public partial class LegacyTabsView : UserControl
{
    private ObservableCollection<DispatchCenter> _dispatchCenters = new();
    private ObservableCollection<Agency> _agencies = new();
    private ObservableCollection<Station> _stations = new();
    private ObservableCollection<Unit> _units = new();

    private string? _currentDispatchCenterCode;
    private int? _currentDispatchCenterId;

    private int? _selectedStationKey;
    private string? _selectedStationIdText;

    public LegacyTabsView()
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
        using var db = new DucommForgeDbContext();

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

        using var db = new DucommForgeDbContext();

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

    private void CurrentDispatchCenterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CurrentDispatchCenterComboBox.ItemsSource == null)
            return;

        var selectedCode = (CurrentDispatchCenterComboBox.SelectedItem as string ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(selectedCode))
            return;

        using var db = new DucommForgeDbContext();
        UpsertAppSetting(db, "CurrentDispatchCenterCode", selectedCode);
        db.SaveChanges();

        _currentDispatchCenterCode = selectedCode;

        ApplyDispatchCenterScopeFromSetting();
        SetScopeUi();
        LoadAgencies();
        LoadStations();

        ConfigStatusText.Text = $"Scope set to {selectedCode}.";
    }

    private static void UpsertAppSetting(DucommForgeDbContext db, string key, string value)
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
        using var db = new DucommForgeDbContext();

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
        using var db = new DucommForgeDbContext();

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

    private void DispatchCenterSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var term = (DispatchCenterSearchTextBox.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            DispatchCentersGrid.ItemsSource = _dispatchCenters;
            return;
        }

        var filtered = _dispatchCenters
            .Where(x => x.Code.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || x.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        DispatchCentersGrid.ItemsSource = new ObservableCollection<DispatchCenter>(filtered);
    }

    private void AddDispatchCenter_Click(object sender, RoutedEventArgs e)
    {
        _dispatchCenters.Add(new DispatchCenter { Code = "NEW", Name = "New Dispatch Center", Active = true });
        DispatchCentersStatusText.Text = "Added row. Edit values, then Save Changes.";
    }

    private void DeleteDispatchCenter_Click(object sender, RoutedEventArgs e)
    {
        var selected = DispatchCentersGrid.SelectedItems.Cast<DispatchCenter>().ToList();
        foreach (var row in selected)
        {
            _dispatchCenters.Remove(row);
        }
        DispatchCentersStatusText.Text = $"Removed {selected.Count} row(s). Click Save Changes to persist.";
    }

    private void SaveDispatchCenters_Click(object sender, RoutedEventArgs e)
    {
        using var db = new DucommForgeDbContext();

        var currentIds = _dispatchCenters.Select(x => x.DispatchCenterId).ToHashSet();
        var dbRows = db.DispatchCenters.ToList();

        var toDelete = dbRows.Where(x => !currentIds.Contains(x.DispatchCenterId)).ToList();
        if (toDelete.Count > 0)
            db.DispatchCenters.RemoveRange(toDelete);

        foreach (var row in _dispatchCenters)
        {
            var existing = db.DispatchCenters.SingleOrDefault(x => x.DispatchCenterId == row.DispatchCenterId);
            if (existing == null)
            {
                db.DispatchCenters.Add(new DispatchCenter
                {
                    Code = row.Code.Trim().ToUpperInvariant(),
                    Name = row.Name.Trim(),
                    Active = row.Active
                });
            }
            else
            {
                existing.Code = row.Code.Trim().ToUpperInvariant();
                existing.Name = row.Name.Trim();
                existing.Active = row.Active;
            }
        }

        try
        {
            db.SaveChanges();
            DispatchCentersStatusText.Text = "Saved.";
            LoadDispatchCenters();
            ApplyDispatchCenterScopeFromSetting();
            SetScopeUi();
            LoadAgencies();
            LoadStations();
        }
        catch (Exception ex)
        {
            DispatchCentersStatusText.Text = ex.Message;
        }
    }

    // --------------------
    // Agencies
    // --------------------
    private void LoadAgencies()
    {
        if (!_currentDispatchCenterId.HasValue)
        {
            _agencies = new ObservableCollection<Agency>();
            AgenciesGrid.ItemsSource = _agencies;
            AgencyColumn.ItemsSource = _agencies;
            return;
        }

        using var db = new DucommForgeDbContext();

        var all = db.Agencies.AsNoTracking()
            .Where(x => x.DispatchCenterId == _currentDispatchCenterId.Value)
            .OrderBy(x => x.Short)
            .ToList();

        _agencies = new ObservableCollection<Agency>(all);
        AgenciesGrid.ItemsSource = _agencies;
        AgenciesStatusText.Text = "";

        AgencyColumn.ItemsSource = _agencies;
    }

    private void AgencySearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var term = (AgencySearchTextBox.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            AgenciesGrid.ItemsSource = _agencies;
            return;
        }

        var filtered = _agencies
            .Where(x => x.Short.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || x.Type.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        AgenciesGrid.ItemsSource = new ObservableCollection<Agency>(filtered);
    }

    private void AddAgency_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentDispatchCenterId.HasValue)
            return;

        _agencies.Add(new Agency
        {
            DispatchCenterId = _currentDispatchCenterId.Value,
            Short = "NEW",
            Name = "New Agency",
            Type = "Fire",
            Owned = true,
            Active = true
        });
        AgenciesStatusText.Text = "Added row. Edit values, then Save Changes.";
    }

    private void DeleteAgency_Click(object sender, RoutedEventArgs e)
    {
        var selected = AgenciesGrid.SelectedItems.Cast<Agency>().ToList();
        foreach (var row in selected)
        {
            _agencies.Remove(row);
        }
        AgenciesStatusText.Text = $"Removed {selected.Count} row(s). Click Save Changes to persist.";
    }

    private void SaveAgencies_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentDispatchCenterId.HasValue)
            return;

        using var db = new DucommForgeDbContext();

        var currentIds = _agencies.Select(x => x.AgencyId).ToHashSet();
        var dbRows = db.Agencies.Where(x => x.DispatchCenterId == _currentDispatchCenterId.Value).ToList();

        var toDelete = dbRows.Where(x => !currentIds.Contains(x.AgencyId)).ToList();
        if (toDelete.Count > 0)
            db.Agencies.RemoveRange(toDelete);

        foreach (var row in _agencies)
        {
            var existing = db.Agencies.SingleOrDefault(x => x.AgencyId == row.AgencyId);
            if (existing == null)
            {
                db.Agencies.Add(new Agency
                {
                    DispatchCenterId = _currentDispatchCenterId.Value,
                    Short = row.Short.Trim().ToUpperInvariant(),
                    Name = row.Name.Trim(),
                    Type = row.Type.Trim(),
                    Owned = row.Owned,
                    Active = row.Active
                });
            }
            else
            {
                existing.Short = row.Short.Trim().ToUpperInvariant();
                existing.Name = row.Name.Trim();
                existing.Type = row.Type.Trim();
                existing.Owned = row.Owned;
                existing.Active = row.Active;
            }
        }

        try
        {
            db.SaveChanges();
            AgenciesStatusText.Text = "Saved.";
            LoadAgencies();
            LoadStations();
        }
        catch (Exception ex)
        {
            AgenciesStatusText.Text = ex.Message;
        }
    }

    // --------------------
    // Stations
    // --------------------
    private void LoadStations()
    {
        if (!_currentDispatchCenterId.HasValue)
        {
            _stations = new ObservableCollection<Station>();
            StationsGrid.ItemsSource = _stations;
            ClearUnitsSelectionState("Select exactly one station to view/edit units.");
            return;
        }

        using var db = new DucommForgeDbContext();

        var agencyIds = db.Agencies.AsNoTracking()
            .Where(x => x.DispatchCenterId == _currentDispatchCenterId.Value)
            .Select(x => x.AgencyId)
            .ToList();

        var all = db.Stations.AsNoTracking()
            .Where(x => agencyIds.Contains(x.AgencyId))
            .OrderBy(x => x.StationId)
            .ToList();

        _stations = new ObservableCollection<Station>(all);
        StationsGrid.ItemsSource = _stations;
        StationsStatusText.Text = "";

        ClearUnitsSelectionState("Select exactly one station to view/edit units.");
    }

    private void StationSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var term = (StationSearchTextBox.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            StationsGrid.ItemsSource = _stations;
            return;
        }

        var filtered = _stations
            .Where(x => x.StationId.Contains(term, StringComparison.OrdinalIgnoreCase)
                     || (x.Esz ?? "").Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        StationsGrid.ItemsSource = new ObservableCollection<Station>(filtered);
    }

    private void AddStation_Click(object sender, RoutedEventArgs e)
    {
        if (_agencies.Count == 0)
        {
            StationsStatusText.Text = "Add at least one agency first.";
            return;
        }

        _stations.Add(new Station
        {
            StationId = "NEW",
            AgencyId = _agencies[0].AgencyId,
            Esz = null,
            Active = true
        });
        StationsStatusText.Text = "Added row. Edit values, then Save Changes.";
    }

    private void DeleteStation_Click(object sender, RoutedEventArgs e)
    {
        var selected = StationsGrid.SelectedItems.Cast<Station>().ToList();
        foreach (var row in selected)
        {
            _stations.Remove(row);
        }
        StationsStatusText.Text = $"Removed {selected.Count} row(s). Click Save Changes to persist.";

        ClearUnitsSelectionState("Select exactly one station to view/edit units.");
    }

    private void SaveStations_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentDispatchCenterId.HasValue)
            return;

        using var db = new DucommForgeDbContext();

        var agencyIds = db.Agencies.AsNoTracking()
            .Where(x => x.DispatchCenterId == _currentDispatchCenterId.Value)
            .Select(x => x.AgencyId)
            .ToList();

        var currentKeys = _stations.Select(x => x.StationKey).ToHashSet();
        var dbRows = db.Stations.Where(x => agencyIds.Contains(x.AgencyId)).ToList();

        var toDelete = dbRows.Where(x => !currentKeys.Contains(x.StationKey)).ToList();
        if (toDelete.Count > 0)
            db.Stations.RemoveRange(toDelete);

        foreach (var row in _stations)
        {
            var existing = db.Stations.SingleOrDefault(x => x.StationKey == row.StationKey);
            if (existing == null)
            {
                db.Stations.Add(new Station
                {
                    AgencyId = row.AgencyId,
                    StationId = row.StationId.Trim().ToUpperInvariant(),
                    Esz = string.IsNullOrWhiteSpace(row.Esz) ? null : row.Esz.Trim(),
                    Active = row.Active
                });
            }
            else
            {
                existing.AgencyId = row.AgencyId;
                existing.StationId = row.StationId.Trim().ToUpperInvariant();
                existing.Esz = string.IsNullOrWhiteSpace(row.Esz) ? null : row.Esz.Trim();
                existing.Active = row.Active;
            }
        }

        try
        {
            db.SaveChanges();
            StationsStatusText.Text = "Saved.";
            LoadStations();
        }
        catch (Exception ex)
        {
            StationsStatusText.Text = ex.Message;
        }
    }

    private void StationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = StationsGrid.SelectedItems.Cast<Station>().ToList();
        if (selected.Count != 1)
        {
            ClearUnitsSelectionState("Select exactly one station to view/edit units.");
            return;
        }

        _selectedStationKey = selected[0].StationKey;
        _selectedStationIdText = selected[0].StationId;
        LoadUnitsForSelectedStation();
    }

    // --------------------
    // Units
    // --------------------
    private void LoadUnitsForSelectedStation()
    {
        if (!_selectedStationKey.HasValue)
        {
            ClearUnitsSelectionState("Select exactly one station to view/edit units.");
            return;
        }

        using var db = new DucommForgeDbContext();
        var all = db.Units.AsNoTracking()
            .Where(x => x.StationKey == _selectedStationKey.Value)
            .OrderBy(x => x.UnitId)
            .ToList();

        _units = new ObservableCollection<Unit>(all);
        UnitsGrid.ItemsSource = _units;

        SetUnitsUiEnabled(true);
        UnitsStatusText.Text = $"Units for {_selectedStationIdText}";
    }

    private void AddUnit_Click(object sender, RoutedEventArgs e)
    {
        if (!_selectedStationKey.HasValue)
            return;

        _units.Add(new Unit
        {
            StationKey = _selectedStationKey.Value,
            UnitId = "NEW",
            Type = "Engine",
            Jump = false,
            Active = true
        });
        UnitsStatusText.Text = "Added row. Edit values, then Save Units.";
    }

    private void DeleteUnit_Click(object sender, RoutedEventArgs e)
    {
        var selected = UnitsGrid.SelectedItems.Cast<Unit>().ToList();
        foreach (var row in selected)
        {
            _units.Remove(row);
        }
        UnitsStatusText.Text = $"Removed {selected.Count} row(s). Click Save Units to persist.";
    }

    private void SaveUnits_Click(object sender, RoutedEventArgs e)
    {
        if (!_selectedStationKey.HasValue)
            return;

        using var db = new DucommForgeDbContext();

        var currentKeys = _units.Select(x => x.UnitKey).ToHashSet();
        var dbRows = db.Units.Where(x => x.StationKey == _selectedStationKey.Value).ToList();

        var toDelete = dbRows.Where(x => !currentKeys.Contains(x.UnitKey)).ToList();
        if (toDelete.Count > 0)
            db.Units.RemoveRange(toDelete);

        foreach (var row in _units)
        {
            var existing = db.Units.SingleOrDefault(x => x.UnitKey == row.UnitKey);
            if (existing == null)
            {
                db.Units.Add(new Unit
                {
                    StationKey = _selectedStationKey.Value,
                    UnitId = row.UnitId.Trim().ToUpperInvariant(),
                    Type = row.Type.Trim(),
                    Jump = row.Jump,
                    Active = row.Active
                });
            }
            else
            {
                existing.UnitId = row.UnitId.Trim().ToUpperInvariant();
                existing.Type = row.Type.Trim();
                existing.Jump = row.Jump;
                existing.Active = row.Active;
            }
        }

        try
        {
            db.SaveChanges();
            UnitsStatusText.Text = "Saved.";
            LoadUnitsForSelectedStation();
        }
        catch (Exception ex)
        {
            UnitsStatusText.Text = ex.Message;
        }
    }

    private void SetUnitsUiEnabled(bool enabled)
    {
        UnitsGrid.IsEnabled = enabled;
        AddUnitButton.IsEnabled = enabled;
        DeleteUnitButton.IsEnabled = enabled;
        SaveUnitsButton.IsEnabled = enabled;
    }

    private void ClearUnitsSelectionState(string status)
    {
        _selectedStationKey = null;
        _selectedStationIdText = null;
        _units = new ObservableCollection<Unit>();
        UnitsGrid.ItemsSource = _units;
        SetUnitsUiEnabled(false);
        UnitsStatusText.Text = status;
    }
}