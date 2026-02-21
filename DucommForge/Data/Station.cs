namespace DucommForge.Data;

public class Station
{
    public string StationId { get; set; } = "";    // e.g., BAF001 (PK)

    public string AgencyShort { get; set; } = "";  // FK -> Agency.Short
    public Agency? Agency { get; set; }

    public string? Esz { get; set; }
    public bool Active { get; set; } = true;
}