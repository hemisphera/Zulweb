using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zulweb.Entities;

[Table("Settings")]
public class SettingsEntity
{
  [Key]
  [Column]
  [StringLength(100)]
  public string Key { get; set; } = string.Empty;

  [Column]
  [StringLength(100)]
  public string? Value { get; set; }
}