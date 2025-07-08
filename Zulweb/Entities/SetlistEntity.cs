using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zulweb.Entities;

[Table("Setlist")]
public class SetlistEntity
{
  [Key]
  [Column]
  public Guid Id { get; set; }

  [Column]
  [StringLength(200)]
  public string Name { get; set; } = string.Empty;

  [Column]
  [StringLength(200)]
  public string Description { get; set; } = string.Empty;
}