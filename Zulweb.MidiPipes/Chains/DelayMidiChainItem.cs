using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Delays the chain execution for the specified number of milliseconds.
/// </summary>
public class DelayMidiChainItem : IMidiChainItem
{
  public int Milliseconds { get; set; }


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    var ms = Milliseconds;
    if (ms > 0)
      await Task.Delay(TimeSpan.FromMilliseconds(ms));
    await next(message);
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
  /// [0]: the length of the delay in milliseconds. Default: 100.
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    Milliseconds = tokens.GetIntToken(0, 100);
  }
}