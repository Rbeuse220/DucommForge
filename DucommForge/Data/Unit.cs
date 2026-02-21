namespace DucommForge.Data;

public class Unit
{
    public int UnitKey { get; set; }               // PK (SQLite integer identity)

    public int StationKey { get; set; }            // FK
    public Station? Station { get; set; }

    public string UnitId { get; set; } = "";       // e.g., E01 (unique per Station)
    public string Type { get; set; } = "";         // e.g., Engine
    public bool Jump { get; set; } = false;
    public bool Active { get; set; } = true;
}