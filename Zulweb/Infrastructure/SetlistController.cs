using System.Diagnostics;
using System.Text.Json;
using Zulweb.Models;

namespace Zulweb.Infrastructure;

public class SetlistController
{
  private readonly ReaperInterface _reaper;
  private readonly List<SetlistItem> _items = [];

  public LoadedSetlistItem[] Items { get; private set; } = [];

  public LoadedSetlistItem? Next => Items.FirstOrDefault();

  public string? Filename { get; private set; }
  public bool Initialized { get; private set; }
  public bool RehearsalMode { get; set; }


  public SetlistController(ReaperInterface reaper)
  {
    _reaper = reaper;
    _reaper.RegionCompleted += (s, e) =>
    {
      if (RehearsalMode) return;
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

  public async Task LoadFromFile()
  {
    if (string.IsNullOrEmpty(Filename)) return;
    _items.Clear();
    var contents = await File.ReadAllTextAsync(Filename);
    _items.AddRange(JsonSerializer.Deserialize<SetlistItem[]>(contents) ?? []);
    await ResetSetlist();
  }

  public async Task LoadFromFile(string path)
  {
    try
    {
      Filename = path;
      await LoadFromFile();
    }
    catch
    {
      Filename = null;
      throw;
    }
  }

  public void Save()
  {
    if (string.IsNullOrEmpty(Filename)) return;

    var contents = JsonSerializer.Serialize(_items);
    File.WriteAllText(Filename, contents);
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