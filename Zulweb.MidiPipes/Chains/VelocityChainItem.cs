using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

public class VelocityChainItem : IMidiChainItem
{
  private ILogger? _logger;

  /// <summary>
  /// Specifies the output velocity range.
  /// </summary>
  public Range? Range { get; set; }

  /// <summary>
  /// Specifies the method how the range is applied.
  /// </summary>
  public VelocityApplicationMethod Method { get; set; } = VelocityApplicationMethod.Limit;


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (message is not ChannelMessage cm)
    {
      await next(message);
      return;
    }

    if (cm.Command != ChannelCommand.NoteOn && cm.Command != ChannelCommand.NoteOff)
    {
      await next(message);
      return;
    }

    var newVelocity = GetNewValue(cm.Data2);
    if (newVelocity == null) return;

    var cm2 = new ChannelMessage(cm.Command, cm.Channel, cm.Data1, (byte)newVelocity);
    _logger?.LogDebug("Modified velocity from {v1} to {v2}", cm.Data2, cm2.Data2);
    await next(cm2);
  }

  private int? GetNewValue(int inputVelocity)
  {
    return Method switch
    {
      VelocityApplicationMethod.Limit => Range?.Limit(inputVelocity),
      VelocityApplicationMethod.Translate => TranslateToRange(inputVelocity),
      VelocityApplicationMethod.Gate => GateRange(inputVelocity),
      _ => null
    };
  }

  private int? GateRange(int inputVelocity)
  {
    return Range?.Matches(inputVelocity) == true
      ? inputVelocity
      : null;
  }

  private int? TranslateToRange(double inputVelocity)
  {
    if (Range == null) return null;
    var inputPercentage = inputVelocity / 127.0;
    var outputRange = Range.Maximum ?? 127 - Range.Minimum;
    return (int)(Range.Minimum + outputRange * inputPercentage);
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
  /// [0]: The target output range of velocities.
  /// [1]: The method that the velocity is applied. Defaults to limit.
  ///      'Limit': Limits values to the output range. Input velocity is clamped to this.
  ///      'Translate': Proportionally translates to the output velocity range based on the input velocity.
  ///      'Gate': Discards messages that fall outside the output velocity.
  /// </summary>
  /// <param name="tokens"></param>
  /// <exception cref="NotImplementedException"></exception>
  public void FromString(string[] tokens)
  {
    Range = tokens.GetRangeToken(0);
    Method = tokens.GetEnumToken<VelocityApplicationMethod>(1);
  }
}