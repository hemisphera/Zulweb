using Hsp.Midi;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes;

public sealed class MidiPipeHost : IAsyncDisposable
{
  private readonly ILoggerFactory _loggerFactory;
  private readonly ILogger _logger;
  private readonly List<VirtualMidiPort> _virtualPorts = [];
  private readonly List<Connection> _connections = [];

  public bool Started { get; private set; }


  public MidiPipeHost(ILoggerFactory loggerFactory)
  {
    _loggerFactory = loggerFactory;
    _logger = _loggerFactory.CreateLogger(typeof(MidiPipeHost));
  }

  public async Task Start(string configurationPath)
  {
    var pipeConfig = MidiPipeConfiguration.Load(configurationPath);
    _logger.LogInformation("Loading MIDI pipe configuration from {path}", configurationPath);
    await Start(pipeConfig);
  }

  public async Task Start(MidiPipeConfiguration config)
  {
    if (Started) throw new InvalidOperationException();

    try
    {
      _virtualPorts.Clear();
      foreach (var portName in config.VirtualPorts ?? [])
      {
        var port = VirtualMidiPort.Create(portName);
        port.Loopback = false;
        _virtualPorts.Add(port);
        _logger.LogInformation("Created virtual port '{name}'.", portName);
      }

      var delay = TimeSpan.FromSeconds(2);
      _logger.LogInformation("Waiting {delay}s for virtual ports to become available", delay.TotalSeconds);
      await Task.Delay(delay);

      _connections.Clear();
      _connections.AddRange(config.Connections ?? []);
      await Task.WhenAll(_connections.Select(a => a.TryConnect(_loggerFactory)));

      Started = true;
      _logger.LogInformation("Connected");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to connect.");
      await Stop();
    }
  }

  public async Task Stop()
  {
    await Task.WhenAll(_connections.Select(a => a.Disconnect()));

    foreach (var virtualPort in _virtualPorts.ToArray())
    {
      virtualPort.Dispose();
      _virtualPorts.Remove(virtualPort);
      _logger.LogInformation("Removed virtual port '{name}'.", virtualPort.Name);
    }

    _logger.LogInformation("Disconnected");
  }


  public async ValueTask DisposeAsync()
  {
    await Stop();
  }
}