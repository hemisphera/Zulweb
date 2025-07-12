using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Specifies a filter for a MIDI message. If the message does not match, it is discarded.
/// </summary>
public class FilterChainItem : IMidiChainItem
{
  private ILogger? _logger;

  /// <summary>
  /// Specifies the channel(s) to allow. If not specified or empty, all channels are allowed.
  /// </summary>
  public Range[]? Channel { get; set; }

  /// <summary>
  /// Specifies the message type(s) to allow. If not specified or empty, all message types are allowed.
  /// </summary>
  public MidiMessageType[]? MessageType { get; set; }

  /// <summary>
  /// Specifies the value(s) to allow. If not specified or empty, all values are allowed.
  /// </summary>
  public Range[]? Data1 { get; set; }

  /// <summary>
  /// Specifies the value(s) to allow. If not specified or empty, all values are allowed.
  /// </summary>
  public Range[]? Data2 { get; set; }


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    var messageMatches =
      ChannelMatches(message) &&
      MessageTypeMatches(message) &&
      Data1Matches(message) &&
      Data2Matches(message);
    _logger?.LogDebug("FilterMidiChainItem: {message} {matches}", message, messageMatches);
    if (!messageMatches) return;
    await next(message);
  }

  private bool ChannelMatches(IMidiMessage message)
  {
    if (Channel == null || Channel.Length == 0) return true;
    if (message is not ChannelMessage cm) return false;
    return Channel.Any(r => r.Matches(cm.Channel));
  }

  private bool MessageTypeMatches(IMidiMessage message)
  {
    if (MessageType == null || MessageType.Length == 0) return true;
    var isSysex = message is SysExMessage or SysCommonMessage or SysRealtimeMessage;
    if (isSysex)
      return MessageType.Contains(MidiMessageType.SysEx);
    if (message is not ChannelMessage cm) return false;
    return cm.Command switch
    {
      ChannelCommand.NoteOff => MessageType.Contains(MidiMessageType.NoteOff),
      ChannelCommand.NoteOn => MessageType.Contains(MidiMessageType.NoteOn),
      ChannelCommand.PolyPressure => MessageType.Contains(MidiMessageType.PolyPressure),
      ChannelCommand.Controller => MessageType.Contains(MidiMessageType.Controller),
      ChannelCommand.ProgramChange => MessageType.Contains(MidiMessageType.ProgramChange),
      ChannelCommand.ChannelPressure => MessageType.Contains(MidiMessageType.ChannelPressure),
      ChannelCommand.PitchWheel => MessageType.Contains(MidiMessageType.PitchWheel),
      _ => false
    };
  }

  private bool Data1Matches(IMidiMessage message)
  {
    if (Data1 == null || Data1.Length == 0) return true;
    return message is ChannelMessage cm && Data1.Any(r => r.Matches(cm.Data1));
  }

  private bool Data2Matches(IMidiMessage message)
  {
    if (Data2 == null || Data2.Length == 0) return true;
    return message is ChannelMessage cm && Data2.Any(r => r.Matches(cm.Data2));
  }

  public Task Initialize(Connection connection, ILogger? logger = null)
  {
    _logger = logger;
    return Task.CompletedTask;
  }

  public Task Deinitialize()
  {
    return Task.CompletedTask;
  }

  /// <summary>
  /// Parameters:
  /// [0]: The message type. Use '*' to allow all values. Can specify multiple values, delimited by '|'.
  /// [1]: One or more ranges for "Channel". Can specify multiple (delimited by '|'). If multiple conditions are given, they are in OR.
  /// [2]: One or more ranges for "Data1". Can specify multiple (delimited by '|'). If multiple conditions are given, they are in OR.
  /// [3]: One or more ranges for "Data2". Can specify multiple (delimited by '|'). If multiple conditions are given, they are in OR.
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    MessageType = tokens.GetEnumTokenMultiple<MidiMessageType>(0);
    Channel = tokens.GetRangeTokenMultiple(1);
    Data1 = tokens.GetRangeTokenMultiple(2);
    Data2 = tokens.GetRangeTokenMultiple(3);
  }
}