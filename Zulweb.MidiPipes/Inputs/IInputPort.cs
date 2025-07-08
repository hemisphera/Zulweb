using Hsp.Midi.Messages;

namespace Zulweb.MidiPipes.Inputs;

public interface IInputPort
{
  Task Connect();

  Task Disconnect();

  event EventHandler<IMidiMessage> MessageReceived;
}