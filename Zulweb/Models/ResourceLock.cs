using System.Diagnostics;

namespace Zulweb.Models;

public class ResourceLock
{
  private readonly string _name;
  private readonly ILogger? _logger;
  private readonly TimeSpan _release;
  private long _releaseTime;
  public bool IsLocked => Stopwatch.GetTimestamp() <= _releaseTime;

  public ResourceLock(string name, TimeSpan? release = null, ILogger? logger = null)
  {
    _name = name;
    _logger = logger;
    _release = release ?? TimeSpan.FromSeconds(0.5);
  }

  public void Touch()
  {
    if (!IsLocked)
    {
      _logger?.LogDebug("Resource {name} locked", _name);
    }

    _releaseTime = Stopwatch.GetTimestamp() + _release.Ticks;
  }

  public async Task Wait(CancellationToken ct = default)
  {
    while (IsLocked && !ct.IsCancellationRequested)
    {
      await Task.Delay(10, ct);
    }
  }
}