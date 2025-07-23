namespace Zulweb.AbletonLink;

public readonly struct LinkState
{
  public double Beat { get; init; }
  public double Phase { get; init; }
  public double Tempo { get; init; }
  public double Quantum { get; init; }
  public double Time { get; init; }
  public int NumPeers { get; init; }
}