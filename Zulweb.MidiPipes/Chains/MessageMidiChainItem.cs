using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

public class MessageMidiChainItem : IMidiChainItem
{
  public ChannelCommand? Command { get; set; }

  public int? Channel { get; set; }

  public int? Data1 { get; set; }

  public int? Data2 { get; set; }


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (message is not ChannelMessage cm)
    {
      await next(message);
      return;
    }

    var newMessage = new ChannelMessage(
      Command ?? cm.Command,
      Channel ?? cm.Channel,
      Data1 ?? cm.Data1,
      Data2 ?? cm.Data2
    );
    await next(newMessage);
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