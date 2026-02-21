namespace DucommForge.Data;

public class DispatchCenter
{
    public int DispatchCenterId { get; set; }      // PK (SQLite integer identity)
    public string Code { get; set; } = "";         // e.g., DUCOMM
    public string Name { get; set; } = "";         // e.g., DuPage Public Safety Communications
    public bool Active { get; set; } = true;
}