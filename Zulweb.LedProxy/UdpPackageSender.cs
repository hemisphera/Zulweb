using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Zulweb.LedProxy;

public sealed class UdpPackageSender : IPackageSender, IDisposable
{
  private readonly UdpClient _client;
  private int _packageCounter;
  private readonly CancellationTokenSource _cts = new();


  public UdpPackageSender(ILogger<UdpPackageSender> logger)
  {
    _client = new UdpClient();
    _client.Connect(new IPEndPoint(IPAddress.Broadcast, Defaults.UdpPort));
    logger.LogDebug("Sending UDP packages to port {Port}", Defaults.UdpPort);

    Task.Run(async () =>
    {
      var token = _cts.Token;
      while (!_cts.IsCancellationRequested)
      {
        await Task.Delay(TimeSpan.FromSeconds(10), token);
        var v = _packageCounter;
        _packageCounter = 0;
        logger.LogDebug("{PackageCounter} packages sent.", v);
      }
    });
  }


  public async Task Send(byte[] data, CancellationToken cancellationToken)
  {
    _packageCounter++;
    await _client.SendAsync(data, cancellationToken);
  }


  public void Dispose()
  {
    _cts.Cancel();
    _client.Dispose();
  }
}