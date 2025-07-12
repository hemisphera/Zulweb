using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Read a variable value from the internal value storage and apply it to the message. 
/// </summary>
public class ReadValueChainItem : IMidiChainItem
{
  public string VariableName { get; set; } = string.Empty;

  public ValueType Type { get; set; }

  public int InitValue { get; set; }


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (message is not ChannelMessage cm) return;
    var value = ValueStorage.Instance.Read(VariableName, InitValue);
    switch (Type)
    {
      case ValueType.Command:
        cm.Command = (ChannelCommand)value;
        break;
      case ValueType.Channel:
        cm.Channel = value;
        break;
      case ValueType.Data1:
        cm.Data1 = value;
        break;
      case ValueType.Data2:
        cm.Data2 = value;
        break;
    }

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
  /// [1]: The property of the MIDI message where to apply the value. Can be "Command", "Channel", "Data1", "Data2"
  /// [2]: Specifies the initial value for the variable, should it not exist. 
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    VariableName = tokens.GetToken(0) ?? throw new Exception("Argument 0 missing.");
    Type = tokens.GetEnumToken<ValueType>(1);
    InitValue = tokens.GetIntToken(2);
  }
}