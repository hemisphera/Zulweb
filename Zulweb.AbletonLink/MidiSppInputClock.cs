using System.Diagnostics;
using Hsp.Midi;
using Hsp.Midi.Messages;

namespace Zulweb.AbletonLink;

public sealed class MidiSppInputClock : IDisposable
{
  private const int MaxSamples = 50;
  private readonly long[] _deltas = Enumerable.Repeat((long)-1, MaxSamples).ToArray();
  private int _head = MaxSamples;
  private IInputMidiDevice? _device;
  private int _currentBpm;
  private long _lastTempoUpdate;
  private long _lastPing = Stopwatch.GetTimestamp();

  public TimeSpan TempoUpdateFrequency { get; set; } = TimeSpan.FromMilliseconds(500);

  public string MidiDeviceName { get; }


  public event EventHandler<double>? TempoChanged;
  public event EventHandler? PlaybackStarted;
  public event EventHandler? PlaybackStopped;


  /// <summary>
  /// Gets the current calculated BPM
  /// </summary>
  public double CurrentBpm
  {
    get => _currentBpm / 100.0;
    private set
    {
      var rounded = (int)Math.Truncate(value * 100.0);
      if (_currentBpm != rounded)
      {
        _currentBpm = rounded;
        if (Stopwatch.GetTimestamp() > _lastTempoUpdate + TempoUpdateFrequency.Ticks)
        {
          _lastTempoUpdate = Stopwatch.GetTimestamp();
          TempoChanged?.Invoke(this, CurrentBpm);
        }
      }
    }
  }


  public MidiSppInputClock(string deviceName)
  {
    //var now = Stopwatch.GetTimestamp();
    //_deltas = Enumerable.Repeat(now, MaxSamples).ToArray();
    MidiDeviceName = deviceName;
  }


  public void Ping()
  {
    var now = Stopwatch.GetTimestamp();
    _head = Move(_head, 1);
    _deltas[_head] = now - _lastPing;
    _lastPing = now;
    CalculateBpm();
  }

  private static int Move(int head, int p1)
  {
    var newHead = head + p1;
    if (newHead < 0) return MaxSamples - 1;
    if (newHead >= MaxSamples) return 0;
    return newHead;
  }

  private void CalculateBpm()
  {
    long sum = 0;
    var counter = 0;
    for (var i = 0; i < MaxSamples; i++)
    {
      if (_deltas[i] <= 0) continue;
      sum += _deltas[i];
      counter += 1;
    }

    var avgInterval = counter > 0 ? sum / counter : 0;
    var intervalInMs = (double)avgInterval / Stopwatch.Frequency;
    var bpm = 0.0;
    if (intervalInMs != 0)
      bpm = 60.0 / (intervalInMs * 24.0);

    CurrentBpm = bpm;
  }

  public void Reset()
  {
    CurrentBpm = 0.0;
  }


  private void MidiMessageHandler(object? sender, IMidiMessage e)
  {
    if (e is not SysRealtimeMessage msg) return;
    switch (msg.SysRealtimeType)
    {
      case SysRealtimeType.Clock:
        Ping();
        break;
      case SysRealtimeType.Start:
      case SysRealtimeType.Continue:
        PlaybackStarted?.Invoke(this, EventArgs.Empty);
        break;
      case SysRealtimeType.Stop:
        PlaybackStopped?.Invoke(this, EventArgs.Empty);
        break;
    }
  }

  public async Task StartAsync()
  {
    _device = InputMidiDevicePool.Instance.Open(MidiDeviceName);
    _device.MessageReceived += MidiMessageHandler;
    await Task.CompletedTask;
  }

  public async Task StopAsync()
  {
    if (_device == null) return;
    _device.MessageReceived -= MidiMessageHandler;
    InputMidiDevicePool.Instance.Close(_device);
    await Task.CompletedTask;
  }

  public void Dispose()
  {
    if (_device != null)
      InputMidiDevicePool.Instance.Close(_device);
  }
}