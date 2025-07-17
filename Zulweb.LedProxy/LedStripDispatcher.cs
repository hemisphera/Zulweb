using Hsp.Midi;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zulweb.LedProxy;

public sealed class LedStripDispatcher : BackgroundService
{
  private readonly ILogger<LedStripDispatcher> _logger;
  private readonly IPackageSender _sender;
  public string? MidiDeviceName { get; }
  private IInputMidiDevice? _device;
  private VirtualMidiPort? _virtualPort;
  public TimeSpan Frequency { get; set; }

  public LedStrip[] Strips { get; }


  public LedStripDispatcher(IOptions<LedStripSettings> settings, ILogger<LedStripDispatcher> logger, IPackageSender sender)
  {
    _logger = logger;
    _sender = sender;
    MidiDeviceName = settings.Value.MidiDeviceName;

    Strips = new LedStrip[settings.Value.StripCount];
    for (byte i = 0; i < Strips.Length; i++)
    {
      Strips[i] = new LedStrip(i);
    }

    Frequency = TimeSpan.FromMilliseconds(settings.Value.UpdateInterval);
  }

  private void MidiMessageHandler(object? sender, IMidiMessage e)
  {
    foreach (var strip in Strips)
    {
      strip.ProcessMessage(e);
    }
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(Frequency, stoppingToken);
      if (_device == null || Strips.Length == 0) continue;
      await Task.WhenAll(Strips.Select(strip => strip.Send(_sender, stoppingToken)));
    }
  }

  public override async Task StartAsync(CancellationToken cancellationToken)
  {
    await CloseMidiDevice();
    await OpenMidiDevice();
    await base.StartAsync(cancellationToken);
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    await CloseMidiDevice();
    await base.StopAsync(cancellationToken);
  }

  private async Task OpenMidiDevice()
  {
    if (string.IsNullOrEmpty(MidiDeviceName)) return;

    _logger.LogInformation("Creating virtual LED MIDI port {name}", MidiDeviceName);
    _virtualPort = VirtualMidiPort.Create(MidiDeviceName);

    _logger.LogInformation("Waiting for LED MIDI port {name}", MidiDeviceName);
    await Task.Delay(TimeSpan.FromSeconds(2));

    _logger.LogInformation("Opening MIDI device '{MidiDeviceName}'", MidiDeviceName);
    _device = InputMidiDevicePool.Instance.Open(MidiDeviceName);
    _device.MessageReceived += MidiMessageHandler;
  }

  private async Task CloseMidiDevice()
  {
    if (_device == null) return;
    _logger.LogInformation("Closing MIDI device '{MidiDeviceName}'", MidiDeviceName);
    _device.MessageReceived -= MidiMessageHandler;
    InputMidiDevicePool.Instance.Close(_device);

    _logger.LogInformation("Removing virtual LED MIDI port {name}", MidiDeviceName);
    _virtualPort?.Dispose();

    await Task.CompletedTask;
  }
}