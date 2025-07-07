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


  private FileInfo GetFile(Guid id)
  {
    var file = new FileInfo(Path.Combine(_root.FullName, $"{id:D}.json"));
    return file;
  }

  public async IAsyncEnumerable<Setlist> List()
  {
    var files = _root.GetFiles("*.json");
    foreach (var file in files)
    {
      using var str = file.OpenRead();
      var item = await JsonSerializer.DeserializeAsync<Setlist>(str) ?? throw new Exception("Failed to load setlist.");
      item.Id = Guid.Parse(Path.GetFileNameWithoutExtension(file.Name));
      yield return item;
    }
  }

  public async Task Delete(string name)
  {
    var setlists = await List().ToArrayAsync();
    var targets = setlists.Where(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    foreach (var target in targets)
    {
      var file = GetFile(target.Id);
      if (file.Exists) file.Delete();
    }
  }

  public async Task Save(Setlist item)
  {
    if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
    var file = GetFile(item.Id);
    using var s = file.Create();
    await JsonSerializer.SerializeAsync(s, item);
  }
}