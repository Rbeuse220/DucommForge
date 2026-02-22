using System.ComponentModel.DataAnnotations;

namespace DucommForge.Data.Entities;

public class Station
{
    [Key]
    public int StationKey { get; set; }

    public int AgencyId { get; set; }

    [Required]
    public string StationId { get; set; } = string.Empty;

    public string? Esz { get; set; }
    public bool Active { get; set; } = true;

    public Agency? Agency { get; set; }
}