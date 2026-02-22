namespace DucommForge.Data.Entities;

public class Agency
{
    public int AgencyId { get; set; }              // PK (SQLite integer identity)

    public int DispatchCenterId { get; set; }      // FK
    public DispatchCenter? DispatchCenter { get; set; }

    public string Short { get; set; } = "";        // e.g., BAF (unique per DispatchCenter)
    public string? Name { get; set; }
    public string Type { get; set; } = "fire";
    public bool Owned { get; set; } = true;
    public bool Active { get; set; } = true;
}