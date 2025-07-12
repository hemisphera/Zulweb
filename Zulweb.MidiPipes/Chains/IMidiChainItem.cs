using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

public interface IMidiChainItem
{
  Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next);
  Task Initialize(Connection connection, ILogger? logger = null);
  Task Deinitialize();
  void FromString(string[] tokens);
}