using System.ComponentModel.DataAnnotations;

namespace DucommForge.Data.Entities;

public class Agency
{
    [Key]
    public int AgencyId { get; set; }

    public int DispatchCenterId { get; set; }

    [Required]
    public string Short { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    public bool Owned { get; set; }
    public bool Active { get; set; } = true;

    public DispatchCenter? DispatchCenter { get; set; }
}