using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Converts a note to a program change message.
/// </summary>
public class NoteToProgramChangeChainItem : IMidiChainItem
{
  /// <summary>
  /// Specifies the channel to use. If null, the channel of the incoming message is used.
  /// </summary>
  public int? Channel { get; set; }

  /// <summary>
  /// Specifies how the program change number should be calculated.
  /// </summary>
  public NoteToCcValueType Value { get; set; }


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (message is not ChannelMessage cm) return;

    var value = 0;
    switch (Value)
    {
      case NoteToCcValueType.NoteNumber:
        value = cm.Data1;
        break;
      case NoteToCcValueType.Velocity:
        value = cm.Data2;
        break;
    }

    var resultMessage = new ChannelMessage(
      ChannelCommand.ProgramChange,
      Channel ?? cm.Channel,
      value);
    await next(resultMessage);
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
  /// [0]: How the value of the program is calculated.
  ///      'NoteNumber': use the note number as value.
  ///      'Velocity': use the notes velocity as value.
  /// [1]: The MIDI channel to emit the CC message on. Use '*' to use the original value.
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    Value = tokens.GetEnumToken<NoteToCcValueType>(0);
    Channel = tokens.GetIntTokenOrNull(1);
  }
}