using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Takes a value from the MIDI message and writes it the internal variable storage.
/// </summary>
public class WriteValueChainItem : IMidiChainItem
{
  public string VariableName { get; set; } = string.Empty;

  public ValueType Type { get; set; }


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (message is not ChannelMessage cm) return;
    var value = 0;
    switch (Type)
    {
      case ValueType.Channel:
        value = cm.Channel;
        break;
      case ValueType.Data1:
        value = cm.Data1;
        break;
      case ValueType.Data2:
        value = cm.Data2;
        break;
    }

    ValueStorage.Instance.Write(VariableName, value);

    await next(cm);
  }

  public Task Initialize(Connection connection, ILogger? logger = null)
  {
    return Task.CompletedTask;
  }

  public Task Deinitialize()
  {
    return Task.CompletedTask;
  }

  /// <summary>
  /// Parameters:
  /// [0]: The name of the variable. This is required.
  /// [1]: The property of the MIDI message where to get the value from. Can be "Command", "Channel", "Data1", "Data2"
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    VariableName = tokens.GetToken(0);
    Type = tokens.GetEnumToken<ValueType>(1);
  }
}