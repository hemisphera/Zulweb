using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Zulweb.Entities;

[Table("SetlistItem")]
[PrimaryKey(nameof(SetlistId), nameof(RegionName))]
public class SetlistItemEntity
{
  [Column]
  public Guid SetlistId { get; set; }

  [Column]
  public string RegionName { get; set; } = string.Empty;

  [Column]
  public bool Disabled { get; set; }

  [Column]
  public int Sequence { get; set; }
}