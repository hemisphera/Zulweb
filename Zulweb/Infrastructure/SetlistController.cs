using Zulweb.Models;

namespace Zulweb.Infrastructure;

public class SetlistController
{
  private readonly ReaperInterface _reaper;
  private readonly ILogger<SetlistController> _logger;
  private Setlist? _setlist;

  public LoadedSetlistItem[] Items { get; private set; } = [];

  public LoadedSetlistItem? Next => Items.FirstOrDefault();

  public string? SetlistName => _setlist?.Name;
  public bool RehearsalMode { get; set; }


  public SetlistController(ReaperInterface reaper, ILogger<SetlistController> logger)
  {
    _reaper = reaper;
    _logger = logger;
    _reaper.RegionCompleted += (s, e) =>
    {
      if (RehearsalMode) return;
      Items = Items.Where(i => !i.RegionName.Equals(e.Name)).ToArray();
    };
  }


  public SetlistItem? Get(string regionName)
  {
    return _setlist?.Items.FirstOrDefault(r => r.RegionName?.Equals(regionName, StringComparison.OrdinalIgnoreCase) == true);
  }

  public async IAsyncEnumerable<LoadedSetlistItem> BuildAll()
  {
    await _reaper.RefreshAll();
    var setlistItems = _setlist?.Items.ToArray() ?? [];
    foreach (var item in setlistItems)
    {
      var region = _reaper.GetRegionByName(item.RegionName);
      if (region == null)
      {
        _logger.LogWarning("Unable to find REAPER region '{name}'", item.RegionName);
        continue;
      }

      yield return new LoadedSetlistItem(region, item);
    }
  }

  public async Task ResetSetlist()
  {
    _reaper.Reconnect();
    var all = await BuildAll().ToListAsync();
    Items = all
      .Where(i => !i.Disabled && !string.IsNullOrEmpty(i.RegionName))
      .OrderBy(i => i.Sequence)
      .ToArray();
  }

  /*
  public async Task ImportFromReaper()
  {
    await _reaper.RefreshAll();
    foreach (var reaperRegion in await _reaper.GetRegions())
    {
      var current = Get(reaperRegion.Name);
      if (current == null)
        _items.Add(new SetlistItem
        {
          RegionName = reaperRegion.Name
        });
    }
  }
  */

  public async Task Load(Setlist setlist)
  {
    _setlist = setlist;
    await ResetSetlist();
    _logger.LogInformation("Loaded setlist {setlistId}: {setlistName}", setlist.Id, setlist.Name);
  }

  public async Task PlayNext()
  {
    var next = Next;
    if (next == null) return;
    await _reaper.GoToRegion(next.RegionName);
    await _reaper.Play();
  }

  public async Task SetNext(LoadedSetlistItem item)
  {
    Items = Items.Where(i => i.Sequence >= item.Sequence).ToArray();
    await Task.CompletedTask;
  }

  public LoadedSetlistItem? GetItemAt(TimeSpan time)
  {
    return Items.FirstOrDefault(i => i.Start <= time && i.End >= time);
  }
}