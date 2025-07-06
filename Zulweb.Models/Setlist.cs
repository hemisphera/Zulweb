namespace Zulweb.Models;

public class Setlist
{
  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public SetlistItem[] Items { get; set; } = [];
}