using System.ComponentModel.DataAnnotations;

namespace DucommForge.Data.Entities;

public class DispatchCenter
{
    [Key]
    public int DispatchCenterId { get; set; }

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;
}