using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

public class OutputMidiChainItem : IMidiChainItem
{
  private ILogger? _logger;
  private IOutputMidiDevice? _device;

  /// <summary>
  /// Specifies the name of the MIDI port to use.
  /// </summary>
  public string? PortName { get; set; } = string.Empty;

  /// <summary>
  /// Specifies whether to pass the message through to the next chain item, if any.
  /// </summary>
  public bool PassThrough { get; set; }


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (_device != null && _logger?.IsEnabled(LogLevel.Debug) == true)
    {
      _logger.LogMidi(_device, message);
    }

    _device?.Send(message);
    await next(message);
  }

  public Task Initialize(Connection connection, ILogger? logger = null)
  {
    if (string.IsNullOrEmpty(PortName) && !string.IsNullOrEmpty(connection.DefaultOutputPort))
      PortName = connection.DefaultOutputPort;
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
}