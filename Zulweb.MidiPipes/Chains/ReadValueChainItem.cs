using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

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
}