using System.Diagnostics;
using System.Text.Json;
using Zulweb.Models;

namespace Zulweb.Infrastructure;

public class SetlistController
{
  private readonly ReaperInterface _reaper;
  private readonly List<SetlistItem> _items = [];
  private long _lastPlayTrigger;

  public LoadedSetlistItem[] Items { get; private set; } = [];

  //public LoadedSetlistItem? LastPlayed { get; private set; }

  //public LoadedSetlistItem? Next => Items.FirstOrDefault(i => i.Sequence > (LastPlayed?.Sequence ?? 0));
  public LoadedSetlistItem? Next => Items.FirstOrDefault();

  public string? Filename { get; private set; }
  public bool Initialized { get; private set; }


  public SetlistController(ReaperInterface reaper)
  {
    _reaper = reaper;
    _reaper.RegionCompleted += (s, e) =>
    {
      Items = Items.Where(i => !i.RegionName.Equals(e.Name)).ToArray();
    };
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

  public async Task ResetSetlist()
  {
    Initialized = false;
    var all = await BuildAll().ToListAsync();
    Items = all
      .Where(i => !i.Disabled && !string.IsNullOrEmpty(i.ReaperRegionName))
      .OrderBy(i => i.Sequence)
      .ToArray();
    Initialized = true;
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

  public async Task LoadFromFile(string path)
  {
    _items.Clear();
    var contents = await File.ReadAllTextAsync(path);
    _items.AddRange(JsonSerializer.Deserialize<SetlistItem[]>(contents) ?? []);
    Filename = path;
    await ResetSetlist();
  }

  public void Save()
  {
    if (string.IsNullOrEmpty(Filename)) return;

    var contents = JsonSerializer.Serialize(_items);
    File.WriteAllText(Filename, contents);
  }

  public async Task PlayNext()
  {
    //UpdateRemaining();
    //var next = Items.FirstOrDefault();
    var next = Next;
    if (next != null)
    {
      _lastPlayTrigger = Stopwatch.GetTimestamp();
      await _reaper.GoToRegion(next.RegionName);
      await _reaper.Start();
    }
  }

  public async Task SetNext(LoadedSetlistItem item)
  {
    Items = Items.Where(i => i.Sequence >= item.Sequence).ToArray();
    await Task.CompletedTask;
  }
}