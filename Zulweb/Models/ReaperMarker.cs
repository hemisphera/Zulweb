namespace Zulweb.Models;

public class ReaperMarker
{
  public int Id { get; }
  public int? AssignedId { get; set; }
  public bool Exists => AssignedId != null;
  public string? Name { get; set; }
  public TimeSpan Start { get; set; } = TimeSpan.Zero;


  public ReaperMarker(int id)
  {
    Id = id;
  }


  public override string ToString()
  {
    return Exists ? $"{Id}: {Name}" : "<null>";
  }
}