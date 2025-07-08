using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

public class ForkChainItem : IMidiChainItem
{
  /// <summary>
  /// The sub-chains to fork the message to.
  /// </summary>
  public IMidiChainItem[][]? SubChains { get; set; }

  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (SubChains == null || SubChains.Length == 0) return;
    await Task.WhenAll(SubChains.Select(async subChain =>
    {
      var runner = new ChainRunner(connection, subChain);
      await runner.Run(message);
    }));
  }

  public async Task Initialize(Connection connection, ILogger? logger = null)
  {
    if (SubChains == null || SubChains.Length == 0) return;
    var subchainItems = SubChains.SelectMany(subChain => subChain).ToArray();
    await Task.WhenAll(subchainItems.Select(subChain => subChain.Initialize(connection, logger)));
  }

  public async Task Deinitialize()
  {
    if (SubChains == null || SubChains.Length == 0) return;
    var subchainItems = SubChains.SelectMany(subChain => subChain).ToArray();
    await Task.WhenAll(subchainItems.Select(subChain => subChain.Deinitialize()));
  }
}