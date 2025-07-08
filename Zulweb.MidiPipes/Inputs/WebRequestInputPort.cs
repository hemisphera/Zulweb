using Hsp.Midi.Messages;

namespace Zulweb.MidiPipes.Inputs;

public class WebRequestInputPort : IInputPort
{
  private bool _connected;
  public string QueueName { get; }

  public WebRequestInputPort(string queueName)
  {
    QueueName = queueName;
  }

  public Task Connect()
  {
    _connected = true;
    return Task.CompletedTask;
  }

  public Task Disconnect()
  {
    _connected = false;
    return Task.CompletedTask;
  }

  public event EventHandler<IMidiMessage>? MessageReceived;

  public void RaiseMessageReceived(IMidiMessage message)
  {
    if (!_connected) return;
    MessageReceived?.Invoke(this, message);
  }
}