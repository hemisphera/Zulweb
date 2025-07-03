using System.Text.Json;
using Zulweb.Models;

namespace Zulweb.Infrastructure;

public class SetlistController
{
  private readonly ReaperInterface _reaper;
  private readonly List<SetlistItem> _items = [];

  public string? Filename { get; private set; }


  public SetlistController(ReaperInterface reaper)
  {
    _reaper = reaper;
  }


  public SetlistItem? Get(string regionName)
  {
    return _items.FirstOrDefault(r => r.RegionName?.Equals(regionName, StringComparison.OrdinalIgnoreCase) == true);
  }

  public async IAsyncEnumerable<LoadedSetlistItem> BuildAll()
  {
    await _reaper.RefreshAll();
    var setlistItems = _items.ToArray();
    foreach (var item in setlistItems)
    {
      yield return new LoadedSetlistItem(
        _reaper.GetRegionByName(item.RegionName),
        item);
    }
  }

  public async Task<LoadedSetlistItem[]> BuildPlaylist()
  {
    var all = await BuildAll().ToListAsync();
    return all
      .Where(i => !i.Disabled && !string.IsNullOrEmpty(i.ReaperRegionName))
      .ToArray();
  }

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

  public void LoadFromFile(string path)
  {
    _items.Clear();
    var contents = File.ReadAllText(path);
    _items.AddRange(JsonSerializer.Deserialize<SetlistItem[]>(contents) ?? []);
    Filename = path;
  }

  public void Save()
  {
    if (string.IsNullOrEmpty(Filename)) return;

    var contents = JsonSerializer.Serialize(_items);
    File.WriteAllText(Filename, contents);
  }
}