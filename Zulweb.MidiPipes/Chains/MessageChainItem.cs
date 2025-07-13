using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Creates a new MIDI message. Allows reuse of original message data.
/// </summary>
public class MessageChainItem : IMidiChainItem
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

  /// <summary>
  /// Parameters:
  /// [0]: The Command of the new message. Use '*' to reuse the value of the original message.
  /// [1]: The Channel of the new message. Use '*' to reuse the value of the original message.
  /// [2]: The Data1 of the new message. Use '*' to reuse the value of the original message.
  /// [3]: The Data2 of the new message. Use '*' to reuse the value of the original message.
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    Command = tokens.GetEnumTokenOrNull<ChannelCommand>(0);
    Channel = tokens.GetIntTokenOrNull(1);
    Data1 = tokens.GetIntTokenOrNull(2);
    Data2 = tokens.GetIntTokenOrNull(3);
  }
}