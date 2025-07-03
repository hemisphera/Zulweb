using System.Diagnostics;

namespace Zulweb.Models;

public class ResourceLock
{
  private readonly TimeSpan _release;
  private long _releaseTime;
  public bool IsLocked => Stopwatch.GetTimestamp() <= _releaseTime;

  public ResourceLock(TimeSpan? release = null)
  {
    _release = release ?? TimeSpan.FromSeconds(0.5);
  }

  public void Touch()
  {
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