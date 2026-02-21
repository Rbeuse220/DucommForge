namespace DucommForge.Data;

public class Agency
{
    public string Short { get; set; } = "";        // e.g., BAF (PK)

    public int DispatchCenterId { get; set; }      // FK
    public DispatchCenter? DispatchCenter { get; set; }

    public string? Name { get; set; }
    public string Type { get; set; } = "fire";
    public bool Owned { get; set; } = true;
    public bool Active { get; set; } = true;
}