using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Writes the message to an output channel.
/// </summary>
public class OutputChainItem : IMidiChainItem
{
  private ILogger? _logger;
  private IOutputMidiDevice? _device;

  /// <summary>
  /// Specifies the name of the MIDI port to use.
  /// </summary>
  public string? PortName { get; set; } = string.Empty;


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (_device != null && _logger?.IsEnabled(LogLevel.Debug) == true)
    {
      _logger.LogDebug("Sent to {port}: {message}", _device?.Name, message);
    }

    _device?.Send(message);
    await next(message);
  }

  public Task Initialize(Connection connection, ILogger? logger = null)
  {
    if (string.IsNullOrEmpty(PortName) && !string.IsNullOrEmpty(connection.DefaultOutputPort))
      PortName = connection.DefaultOutputPort;
    if (string.IsNullOrEmpty(PortName)) throw new NotSupportedException("No port specified.");
    _device = OutputMidiDevicePool.Instance.Open(PortName);
    _logger = logger;
    return Task.CompletedTask;
  }

  public async Task Deinitialize()
  {
    if (_device == null) return;
    OutputMidiDevicePool.Instance.Close(_device);
    await Task.CompletedTask;
  }

  /// <summary>
  /// Parameters:
  /// [0]: The name of the device to emit the message on. Enclose in double quotes if name has spaces.
  ///      If this is not specified, the default output port of the connection is used (if any).
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    PortName = tokens.GetToken(0);
  }
}