namespace DucommForge.Data;

public class Unit
{
    public string UnitId { get; set; } = "";   // e.g., E01

    public string StationId { get; set; } = "";
    public Station? Station { get; set; }

    public string Type { get; set; } = "";     // Engine, Medic, etc.
    public bool Jump { get; set; } = false;
    public bool Active { get; set; } = true;
}