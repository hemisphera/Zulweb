namespace Zulweb.Models;

public class LoadedSetlistItem
{
  private readonly SetlistItem _item;

  public string RegionName { get; }

  public bool Disabled
  {
    get => _item.Disabled;
    set => _item.Disabled = value;
  }

  public int Sequence
  {
    get => _item.Sequence;
    set => _item.Sequence = value;
  }

  public TimeSpan Start { get; }

  public TimeSpan End { get; }
  public TimeSpan Length => End - Start;


  public LoadedSetlistItem(ReaperRegion reaperRegion, SetlistItem item)
  {
    _item = item;
    RegionName = reaperRegion.Name;
    Start = reaperRegion.Start;
    End = reaperRegion.End;
  }
}