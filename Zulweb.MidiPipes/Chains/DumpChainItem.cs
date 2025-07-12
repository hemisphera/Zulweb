using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;
using Zulweb.MidiPipes.Inputs;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Dumps the MIDI message to the log output.
/// </summary>
public class DumpChainItem : IMidiChainItem
{
  private ILogger? _logger;

  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    _logger?.LogMidi((connection.Port as MidiInputPort)?.Device, message, LogLevel.Information);
    await next(message);
  }

  public Task Initialize(Connection connection, ILogger? logger = null)
  {
    _logger = logger;
    return Task.CompletedTask;
  }

  public Task Deinitialize()
  {
    _logger = null;
    return Task.CompletedTask;
  }

  /// <summary>
  /// Parameters:
  /// none
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
  }
}