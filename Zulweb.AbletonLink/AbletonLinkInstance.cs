using System.Runtime.InteropServices;

namespace Zulweb.AbletonLink;

public sealed class AbletonLinkInstance : IDisposable
{
  private IntPtr _nativeInstance = IntPtr.Zero;
  private const double InitialTempo = 120.0;


  [DllImport("AbletonLinkDLL")]
  private static extern IntPtr CreateAbletonLink();


  public AbletonLinkInstance(double initialTempo = InitialTempo)
  {
    _nativeInstance = CreateAbletonLink();
    Setup(initialTempo);
  }

  [DllImport("AbletonLinkDLL")]
  private static extern void DestroyAbletonLink(IntPtr ptr);


  public void Dispose()
  {
    if (_nativeInstance == IntPtr.Zero) return;
    DestroyAbletonLink(_nativeInstance);
    _nativeInstance = IntPtr.Zero;
  }

  [DllImport("AbletonLinkDLL")]
  private static extern void setup(IntPtr ptr, double bpm);

  private void Setup(double bpm)
  {
    setup(_nativeInstance, bpm);
  }

  [DllImport("AbletonLinkDLL")]
  private static extern void setTempo(IntPtr ptr, double bpm);


  [DllImport("AbletonLinkDLL")]
  private static extern double tempo(IntPtr ptr);

  public double Tempo
  {
    get => tempo(_nativeInstance);
    set => setTempo(_nativeInstance, value);
  }


  [DllImport("AbletonLinkDLL")]
  private static extern void setQuantum(IntPtr ptr, double quantum);


  [DllImport("AbletonLinkDLL")]
  private static extern double quantum(IntPtr ptr);

  public double Quantum
  {
    get => quantum(_nativeInstance);
    set => setQuantum(_nativeInstance, value);
  }

  [DllImport("AbletonLinkDLL")]
  private static extern void forceBeatAtTime(IntPtr ptr, double beat);

  public void ForceBeatAtTime(double beat)
  {
    forceBeatAtTime(_nativeInstance, beat);
  }


  [DllImport("AbletonLinkDLL")]
  private static extern void requestBeatAtTime(IntPtr ptr, double beat);

  public void RequestBeatAtTime(double beat)
  {
    requestBeatAtTime(_nativeInstance, beat);
  }

  [DllImport("AbletonLinkDLL")]
  private static extern void enable(IntPtr ptr, bool bEnable);

  [DllImport("AbletonLinkDLL")]
  private static extern bool isEnabled(IntPtr ptr);

  public bool Enabled
  {
    get => isEnabled(_nativeInstance);
    set => enable(_nativeInstance, value);
  }


  [DllImport("AbletonLinkDLL")]
  private static extern void enableStartStopSync(IntPtr ptr, bool bEnable);

  public void EnableStartStopSync(bool enable)
  {
    enableStartStopSync(_nativeInstance, enable);
  }


  [DllImport("AbletonLinkDLL")]
  private static extern void startPlaying(IntPtr ptr);

  public void StartPlaying()
  {
    startPlaying(_nativeInstance);
  }


  [DllImport("AbletonLinkDLL")]
  private static extern void stopPlaying(IntPtr ptr);

  public void StopPlaying()
  {
    stopPlaying(_nativeInstance);
  }


  [DllImport("AbletonLinkDLL")]
  private static extern bool isPlaying(IntPtr ptr);

  public bool IsPlaying => isPlaying(_nativeInstance);


  [DllImport("AbletonLinkDLL")]
  private static extern int numPeers(IntPtr ptr);

  public int NumPeers => numPeers(_nativeInstance);


  [DllImport("AbletonLinkDLL")]
  private static extern void update(IntPtr ptr, out double rbeat, out double rphase, out double rtempo, out double rquantum, out double rtime, out int rnumPeers);

  public LinkState GetState()
  {
    update(_nativeInstance, out var beat, out var phase, out var tempo, out var quantum, out var time, out var numPeers);
    return new LinkState
    {
      Beat = beat,
      Phase = phase,
      Tempo = tempo,
      Quantum = quantum,
      Time = time,
      NumPeers = numPeers
    };
  }
}