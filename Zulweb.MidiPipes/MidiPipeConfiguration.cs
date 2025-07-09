using System.Text.Json;
using System.Text.Json.Serialization;
using Hsp.Midi;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes;

public class MidiPipeConfiguration
{
  private static readonly JsonSerializerOptions SerializerOptions = new()
  {
    Converters =
    {
      new MidiChainItemJsonConverter(),
      new ChannelMessageJsonConverter(),
      new RangeJsonConverter(),
      new JsonStringEnumConverter()
    }
  };

  [JsonIgnore]
  public ILoggerFactory? LoggerFactory { get; set; }

  public bool Started { get; private set; }


  /// <summary>
  /// Specifies a list of virtual MIDI ports to create.
  /// Theese virtual ports will NOT loopback.
  /// </summary>
  public string[]? VirtualPorts { get; set; }

  /// <summary>
  /// Specifies a list of virtual MIDI ports to create.
  /// Theese virtual ports WILL loopback.
  /// </summary>
  public string[]? VirtualLoopbackPorts { get; set; }

  /// <summary>
  /// Specifies a list of MIDI connections to create.
  /// </summary>
  public Connection[]? Connections { get; set; }


  public static MidiPipeConfiguration Load(string? path)
  {
    ArgumentException.ThrowIfNullOrEmpty(path);
    using var s = File.OpenRead(path);
    return
      JsonSerializer.Deserialize<MidiPipeConfiguration>(s, SerializerOptions)
      ?? throw new Exception($"Failed to load configuration from '{path}'.");
  }
}