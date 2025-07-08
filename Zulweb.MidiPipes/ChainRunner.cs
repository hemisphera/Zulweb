using Hsp.Midi.Messages;
using Zulweb.MidiPipes.Chains;

namespace Zulweb.MidiPipes;

internal class ChainRunner
{
  private readonly Connection _conn;
  private readonly IMidiChainItem[] _chain;


  public ChainRunner(Connection conn, IEnumerable<IMidiChainItem> chain)
  {
    _conn = conn;
    _chain = chain.ToArray();
  }


  public async Task Run(IMidiMessage msg)
  {
    if (_chain.Length == 0) return;

    var item = _chain.First();
    var nextChain = _chain.Skip(1).ToArray();

    await item.ProcessAsync(_conn, msg, NextFunc);
    return;

    async Task NextFunc(IMidiMessage nextMsg)
    {
      var cr = new ChainRunner(_conn, nextChain);
      await cr.Run(nextMsg);
    }
  }
}