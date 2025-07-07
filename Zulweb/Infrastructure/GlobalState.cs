using System.Text.Json;

namespace Zulweb.Infrastructure;

public class GlobalState
{
  public static string RootFolder { get; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Zulweb");


  private static Dictionary<string, string> _values = new();
  private static DateTime _lastRead = DateTime.MinValue;
  private static readonly FileInfo StateFile = new(Path.Combine(RootFolder, "state.json"));

  public static Guid LastSetlistId
  {
    get => Guid.TryParse(GetValue(nameof(LastSetlistId)), out var id) ? id : Guid.Empty;
    set => SetValue(nameof(LastSetlistId), value.ToString("D"));
  }

  private static string GetValue(string name)
  {
    LoadDictionary();
    return _values.TryGetValue(name, out var value) ? value : string.Empty;
  }

  private static void SetValue(string propertyName, string propertyValue)
  {
    LoadDictionary();
    _values[propertyName] = propertyValue;
    SaveDictionary();
  }

  private static void SaveDictionary()
  {
    StateFile.Directory?.Create();
    var contents = JsonSerializer.Serialize(StateFile.FullName);
    File.WriteAllText(StateFile.FullName, contents);
    StateFile.Refresh();
    _lastRead = StateFile.LastWriteTimeUtc;
  }

  private static void LoadDictionary()
  {
    StateFile.Refresh();
    if (!StateFile.Exists) return;
    if (StateFile.LastWriteTimeUtc < _lastRead) return;
    var contents = File.ReadAllText(StateFile.FullName);
    _values = JsonSerializer.Deserialize<Dictionary<string, string>>(contents) ?? [];
    _lastRead = DateTime.UtcNow;
  }
}