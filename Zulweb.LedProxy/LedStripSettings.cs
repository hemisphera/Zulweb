namespace Zulweb.LedProxy;

public class LedStripSettings
{
  public string? MidiDeviceName { get; set; }
  public int StripCount { get; set; } = 4;
  public int UpdateInterval { get; set; } = 10;
}