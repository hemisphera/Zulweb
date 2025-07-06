using System.Diagnostics;
using System.Net;
using Hsp.Osc;
using Zulweb.Infrastructure.EventArgs;
using Zulweb.Infrastructure.Settings;
using Zulweb.Models;

namespace Zulweb.Infrastructure;

public sealed class ReaperInterface : IAsyncDisposable
{
  private ReaperRegion[] _regions = [];
  private ReaperMarker[] _markers = [];
  private double _time;

  public ResourceLock RegionLock { get; } = new();
  public ResourceLock MarkerLock { get; } = new();

  public ReaperRegion? CurrentRegion
  {
    get => _currentRegion;
    set
    {
      if (_currentRegion == value) return;
      var oldRegion = _currentRegion;
      _currentRegion = value;
      RegionChanged?.Invoke(this, new RegionChangedEventArgs(oldRegion, _currentRegion));
    }
  }

  public bool Playing
  {
    get => _playing;
    set
    {
      if (value == _playing) return;
      _playing = value;
      _lastPlayStateChange = Stopwatch.GetTimestamp();
      PlayStateChanged?.Invoke(this, _playing);
    }
  }

  public TimeSpan Time => TimeSpan.FromSeconds(_time);

  public bool Connected { get; private set; }


  private OscUdpClient? _sender;
  private OscUdpServer? _receiver;
  private ReaperRegion? _currentRegion;
  private bool _playing;
  private long _lastPlayStateChange;
  private readonly TimeSpan _autoStopAfter = TimeSpan.FromMilliseconds(500);


  public event EventHandler<RegionChangedEventArgs>? RegionChanged;
  public event EventHandler<bool>? PlayStateChanged;
  public event EventHandler<ReaperRegion>? RegionCompleted;


  // ReSharper disable once UnusedMember.Local
  [OscMessageHandler(@"^\/region\/(?<index>[0-9]+)\/(?<prop>.*)")]
  private void InboundMarkerHandler(MessageHandlerContext arg)
  {
    RegionLock.Touch();

    var index = int.Parse(arg.Match.Groups["index"].Value) - 1;
    var prop = arg.Match.Groups["prop"].Value;
    if (index < 0 || index >= _regions.Length) return;

    var region = _regions[index];
    if (prop == "name")
      region.Name = arg.Message.Atoms[0].StringValue ?? string.Empty;
    if (prop == "time")
      region.Start = TimeSpan.FromSeconds(arg.Message.Atoms[0].Float32Value);
    if (prop == "number/str")
      region.Exists = !string.IsNullOrEmpty(arg.Message.Atoms[0].StringValue);
    if (prop == "length")
      region.Duration = TimeSpan.FromSeconds(arg.Message.Atoms[0].Float32Value);
  }

  // ReSharper disable once UnusedMember.Local
  [OscMessageHandler(@"\/marker\/(?<index>[0-9]+)\/(?<prop>.*)")]
  private void InboundRegionHandler(MessageHandlerContext arg)
  {
    MarkerLock.Touch();

    var index = int.Parse(arg.Match.Groups["index"].Value) - 1;
    var prop = arg.Match.Groups["prop"].Value;
    if (index < 0 || index >= _markers.Length) return;

    var marker = _markers[index];
    if (prop == "name")
      marker.Name = arg.Message.Atoms[0].StringValue;
    if (prop == "time")
      marker.Start = TimeSpan.FromSeconds(arg.Message.Atoms[0].Float32Value);
    if (prop == "number/str")
    {
      var strVal = arg.Message.Atoms[0].StringValue;
      marker.AssignedId = !string.IsNullOrEmpty(strVal) ? int.Parse(strVal) : null;
    }
  }

  // ReSharper disable once UnusedMember.Local
  [OscMessageHandler("/lastregion/number/str")]
  private void InboundLastRegionHandler(MessageHandlerContext arg)
  {
    if (!int.TryParse(arg.Message.Atoms[0].StringValue, out var id)) id = -1;
    var oldRegion = CurrentRegion;
    var newRegion = GetRegionById(id);
    CurrentRegion = newRegion;
    if (CanAutoStop() && oldRegion != newRegion && oldRegion != null)
    {
      Task.Run(async () =>
      {
        await StopAndMoveCursor();
        RegionCompleted?.Invoke(this, oldRegion);
      });
    }
  }

  private bool CanAutoStop()
  {
    if (!Playing) return false;
    return Stopwatch.GetTimestamp() - _lastPlayStateChange > _autoStopAfter.Ticks;
  }

  // ReSharper disable once UnusedMember.Local
  [OscMessageHandler("^/play$")]
  private void InboundPlayHandler(MessageHandlerContext arg)
  {
    var v = arg.Message.Atoms[0].TypeTag == TypeTag.OscInt32
      ? arg.Message.Atoms[0].Int32Value
      : (int)arg.Message.Atoms[0].Float32Value;
    Playing = v == 1;
  }

  // ReSharper disable once UnusedMember.Local
  [OscMessageHandler("^/time$")]
  private void InboundTimeHandler(MessageHandlerContext arg)
  {
    _time = arg.Message.Atoms[0].Float32Value;
  }

  public ReaperRegion? GetRegionByName(string? strVal)
  {
    return _regions
      .Where(r => r.Exists)
      .FirstOrDefault(r => r.Name?.Equals(strVal, StringComparison.OrdinalIgnoreCase) == true);
  }

  public ReaperRegion? GetRegionById(int id)
  {
    return _regions
      .Where(r => r.Exists)
      .FirstOrDefault(r => r.Id == id);
  }

  public async Task<ReaperMarker[]> GetRegionMarkers(string regionName)
  {
    var region = GetRegionByName(regionName);
    if (region == null) return [];
    return await GetRegionMarkers(region);
  }

  public async Task<ReaperMarker[]> GetRegionMarkers(ReaperRegion region)
  {
    var result = _markers
      .Where(m => m.Exists)
      .Where(m => m.Start >= region.Start && m.Start <= region.End)
      .ToArray();
    return await Task.FromResult(result);
  }

  public async Task<ReaperRegion[]> GetRegions()
  {
    return await Task.FromResult(_regions.Where(r => r.Exists).ToArray());
  }

  public async Task<ReaperMarker[]> GetMarkers()
  {
    return await Task.FromResult(_markers.Where(r => r.Exists).ToArray());
  }


  private OscUdpClient EnsureClient()
  {
    return _sender ?? throw new InvalidOperationException("Sender is not connected.");
  }


  public async Task ConnectAsync(ReaperSettings settings)
  {
    if (Connected) throw new InvalidOperationException("REAPER is already connected.");

    _regions = Enumerable.Range(0, settings.RegionCount).Select(i => new ReaperRegion(i + 1)).ToArray();
    _markers = Enumerable.Range(0, settings.MarkerCount).Select(i => new ReaperMarker(i + 1)).ToArray();

    _sender = new OscUdpClient(settings.OscPort + 10, IPAddress.Loopback, settings.OscPort);
    _receiver = new OscUdpServer(IPAddress.Loopback, _sender.LocalPort);
    _receiver.RegisterHandlers(this);

    _receiver.BeginListen();

    await _sender.ConnectAsync();
    await _sender.SendMessageAsync(new Message("/device/marker/count").PushAtom(_markers.Length));
    await _sender.SendMessageAsync(new Message("/device/region/count").PushAtom(_regions.Length));

    await RefreshAll();

    Connected = true;
  }

  public async Task DisconnectAsync()
  {
    if (!Connected) throw new InvalidOperationException("REAPER is already connected.");

    _receiver?.EndListen();
    _receiver?.Dispose();

    if (_sender != null)
    {
      await _sender.DisconnectAsync();
      _sender.Dispose();
    }

    Connected = false;
  }


  public async Task Play()
  {
    await EnsureClient().SendMessageAsync(new Message("/play"));
  }

  public async Task Stop()
  {
    await EnsureClient().SendMessageAsync(new Message("/stop"));
  }

  public Task Pause()
  {
    //throw new NotImplementedException();
    return Task.CompletedTask;
  }

  public async Task StopAndMoveCursor()
  {
    await EnsureClient().SendMessageAsync(new Message("/action").PushAtom(40328));
  }


  public async Task RefreshAll()
  {
    await EnsureClient().SendMessageAsync(new Message("/action").PushAtom(41743)); // refresh control surfaces
    RegionLock.Touch();
    MarkerLock.Touch();
    await Task.WhenAll(RegionLock.Wait(), MarkerLock.Wait());
  }


  public async ValueTask DisposeAsync()
  {
    await DisconnectAsync();
    _sender?.Dispose();
    _receiver?.Dispose();
  }

  public async Task GoToRegion(string name)
  {
    var region = GetRegionByName(name);
    if (region == null) return;
    await EnsureClient().SendMessageAsync(new Message("/region").PushAtom(region.Id));
  }

  public async Task GoToMarker(ReaperMarker marker)
  {
    await EnsureClient().SendMessageAsync(new Message("/marker").PushAtom(marker.Id));
  }

  public async Task GoToTime(TimeSpan time)
  {
    await EnsureClient().SendMessageAsync(new Message("/time").PushAtom((float)time.TotalSeconds));
  }
}