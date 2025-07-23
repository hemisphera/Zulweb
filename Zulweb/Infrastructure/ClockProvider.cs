using System.Diagnostics;
using Hsp.Midi;
using Microsoft.Extensions.Options;
using Zulweb.AbletonLink;

namespace Zulweb.Infrastructure;

public sealed class ClockProvider : BackgroundService
{
  private readonly ILogger<ClockProvider> _logger;
  private AbletonLinkInstance? _instance;
  private MidiSppInputClock? _externalClock;
  public bool Enabled { get; }
  public string InputMidiDevice { get; }
  private long _lastTempoUpdate = 0;
  private VirtualMidiPort? _virtualPort;


  public ClockProvider(IOptions<Settings.AbletonLink> settings, ILogger<ClockProvider> logger)
  {
    _logger = logger;
    InputMidiDevice = settings.Value.MidiDeviceName ?? string.Empty;
    Enabled = settings.Value.Enabled && !string.IsNullOrEmpty(InputMidiDevice);
  }


  public override async Task StartAsync(CancellationToken cancellationToken)
  {
    if (!Enabled) return;
    _logger.LogInformation("Starting AbletonLink bridge on {device}.", InputMidiDevice);

    _logger.LogInformation("Creating virtual port {name}", InputMidiDevice);
    _virtualPort = VirtualMidiPort.Create(InputMidiDevice);

    _logger.LogInformation("Waiting for virtual port {name}", InputMidiDevice);
    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

    _instance = new AbletonLinkInstance();
    _externalClock = new MidiSppInputClock(InputMidiDevice);
    _externalClock.PlaybackStarted += PlaybackStartedHandler;
    _externalClock.PlaybackStopped += PlaybackStoppedHandler;
    _externalClock.TempoChanged += TempoChangedHandler;
    await _externalClock.StartAsync();

    await base.StartAsync(cancellationToken);
  }

  private void TempoChangedHandler(object? sender, double e)
  {
    if (_instance == null) return;
    // only propagate tempo every 0,5s
    if (Stopwatch.GetElapsedTime(_lastTempoUpdate).TotalMilliseconds < 500) return;

    _logger.LogDebug("Tempo changed to {tempo}", e);
    _logger.LogDebug("Playing {is}", _instance.IsPlaying);
    _instance.Tempo = e;
    _lastTempoUpdate = Stopwatch.GetTimestamp();
  }

  private void PlaybackStartedHandler(object? sender, System.EventArgs e)
  {
    _instance?.StartPlaying();
    _logger.LogDebug("Playback started.");
  }

  private void PlaybackStoppedHandler(object? sender, System.EventArgs e)
  {
    _instance?.StopPlaying();
    _logger.LogDebug("Playback stopped.");
  }


  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var nop = TimeSpan.FromSeconds(2);
    if (!Enabled)
    {
      _logger.LogDebug("AbletonLink is disabled.");
    }

    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(nop, stoppingToken);
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    if (_instance == null) return;

    _logger.LogInformation("Stopping clock");
    _instance.Enabled = false;
    await StopClock();

    await base.StopAsync(cancellationToken);
  }

  private async Task StopClock()
  {
    if (_externalClock == null) return;

    await _externalClock.StopAsync();
    _externalClock.PlaybackStarted -= PlaybackStartedHandler;
    _externalClock.PlaybackStopped -= PlaybackStoppedHandler;
    _externalClock.TempoChanged -= TempoChangedHandler;
  }

  public override void Dispose()
  {
    _externalClock?.Dispose();
    _virtualPort?.Dispose();
    base.Dispose();
  }
}