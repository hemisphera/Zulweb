using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Inputs;

public class MidiInputPort : IInputPort
{
  private readonly ILogger? _logger;
  public IInputMidiDevice? Device { get; private set; }
  public string PortName { get; }


  public event EventHandler<IMidiMessage>? MessageReceived;


  public MidiInputPort(string portName, ILogger? logger)
  {
    _logger = logger;
    PortName = portName;
  }


  public async Task Connect()
  {
    ArgumentException.ThrowIfNullOrEmpty(PortName, nameof(PortName));
    Device = InputMidiDevicePool.Instance.Open(PortName);
    Device.MessageReceived += DeviceOnMessageReceived;
    await Task.CompletedTask;
  }

  private void DeviceOnMessageReceived(object? sender, IMidiMessage e)
  {
    MessageReceived?.Invoke(this, e);
  }

  public async Task Disconnect()
  {
    if (Device == null) return;
    InputMidiDevicePool.Instance.Close(Device);
    await Task.CompletedTask;
  }
}