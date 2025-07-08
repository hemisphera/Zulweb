using Hsp.Midi;
using Hsp.Midi.Messages;

namespace Zulweb.MidiPipes.Inputs;

public class MidiInputPort : IInputPort
{
  public InputMidiDevice? Device { get; private set; }
  public string PortName { get; }


  public event EventHandler<IMidiMessage>? MessageReceived;


  public MidiInputPort(string portName)
  {
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