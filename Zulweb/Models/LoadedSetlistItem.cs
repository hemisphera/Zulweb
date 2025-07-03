using Zulweb.Infrastructure;

namespace Zulweb.Models;

public class LoadedSetlistItem
{
  private readonly SetlistItem _item;

  public string RegionName
  {
    get => _item.RegionName;
    set => _item.RegionName = value;
  }

  public string? ReaperRegionName { get; set; }

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


  public LoadedSetlistItem(ReaperRegion? reaperRegion, SetlistItem item)
  {
    _item = item;
    ReaperRegionName = reaperRegion?.Name;
    Start = reaperRegion?.Start ?? TimeSpan.Zero;
    End = reaperRegion?.End ?? TimeSpan.Zero;
  }
}