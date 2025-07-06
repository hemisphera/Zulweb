using System.Text.Json;
using Zulweb.Models;

namespace Zulweb.Infrastructure;

public class JsonSetlistStorage : ISetlistStorage
{
  private readonly DirectoryInfo _root;

  public JsonSetlistStorage()
  {
    _root = new DirectoryInfo(Path.Combine(GlobalState.RootFolder, "setlists"));
    _root.Create();
  }


  public async Task<string[]> List()
  {
    var files = _root.GetFiles("*.json");
    return await Task.FromResult(files.Select(f => f.Name).ToArray());
  }

  public async Task Delete(string name)
  {
    var file = new FileInfo(Path.Combine(_root.FullName, $"{name}.json"));
    file.Delete();
    await Task.CompletedTask;
  }

  public async Task<Setlist> Load(string name)
  {
    var file = new FileInfo(Path.Combine(_root.FullName, $"{name}.json"));
    using var s = file.OpenRead();
    var setlist = await JsonSerializer.DeserializeAsync<Setlist>(s) ?? throw new Exception("Failed to load setlist.");
    setlist.Name = file.Name;
    return setlist;
  }

  public async Task Save(string name, Setlist item)
  {
    item.Name = name;
    var file = new FileInfo(Path.Combine(_root.FullName, $"{item.Name}.json"));
    using var s = file.Create();
    await JsonSerializer.SerializeAsync(s, item);
  }
}