namespace DucommForge.Data.Entities;

public class Station
{
    public int StationKey { get; set; }            // PK (SQLite integer identity)

    public int AgencyId { get; set; }              // FK
    public Agency? Agency { get; set; }

    public string StationId { get; set; } = "";    // e.g., BAF001 (unique per Agency)
    public string? Esz { get; set; }
    public bool Active { get; set; } = true;
}