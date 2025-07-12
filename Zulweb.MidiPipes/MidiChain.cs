using Microsoft.Extensions.Logging;
using Zulweb.MidiPipes.Chains;

namespace Zulweb.MidiPipes;

public class MidiChain
{
  public bool Enabled { get; set; } = true;
  public IMidiChainItem[]? Items { get; set; }


  public async Task Initialize(Connection connection, ILoggerFactory? loggerFactory)
  {
    var logger = loggerFactory?.CreateLogger<MidiChain>();
    await Task.WhenAll((Items ?? []).Select(a =>
    {
      try
      {
        return a.Initialize(connection, loggerFactory?.CreateLogger(a.GetType()));
      }
      catch (Exception ex)
      {
        logger?.LogError(ex, "Failed to initialize chain item {name}: {message}", a.GetType().Name, ex.Message);
        throw;
      }
    }));
  }

  public async Task Deinitialize()
  {
    await Task.WhenAll((Items ?? []).Select(c => c.Deinitialize()));
  }
}