using System.Collections.Concurrent;

namespace Zulweb.MidiPipes;

public class ValueStorage
{
  public static ValueStorage Instance { get; } = new();

  private readonly ConcurrentDictionary<string, int> _storage = new();


  private ValueStorage()
  {
  }

  public int Read(string variableName, int initValue)
  {
    return _storage.GetValueOrDefault(variableName, initValue);
  }

  public void Write(string variableName, int value)
  {
    _storage.AddOrUpdate(variableName, _ => value, (_, _) => value);
  }
}