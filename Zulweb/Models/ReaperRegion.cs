namespace Zulweb.Models;

public class ReaperRegion
{
  public int Id { get; }
  public bool Exists { get; set; }
  public string Name { get; set; } = string.Empty;
  public TimeSpan Start { get; set; } = TimeSpan.Zero;
  public TimeSpan Duration { get; set; } = TimeSpan.Zero;
  public TimeSpan End => Start.Add(Duration);


  public ReaperRegion(int index)
  {
    Id = index;
  }


  public override string ToString()
  {
    return Exists ? $"{Id}: {Name}" : "<null>";
  }
}