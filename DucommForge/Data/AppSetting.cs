using System.ComponentModel.DataAnnotations;

namespace DucommForge.Data;

public class AppSetting
{
    [Key]
    public int AppSettingId { get; set; }

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}