using System.ComponentModel.DataAnnotations;

namespace DucommForge.Data.Entities;

public class Unit
{
    [Key]
    public int UnitKey { get; set; }

    public int StationKey { get; set; }

    [Required]
    public string UnitId { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    public bool Jump { get; set; }
    public bool Active { get; set; } = true;

    public Station? Station { get; set; }
}