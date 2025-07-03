using Zulweb.Models;

namespace Zulweb.Infrastructure.EventArgs;

public class RegionChangedEventArgs
{
  public ReaperRegion? OldRegion { get; }
  public ReaperRegion? NewRegion { get; }


  public RegionChangedEventArgs(ReaperRegion? oldRegion, ReaperRegion? newRegion)
  {
    OldRegion = oldRegion;
    NewRegion = newRegion;
  }
}