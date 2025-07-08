using System.Text.Json;
using System.Text.Json.Serialization;
using Hsp.Midi;
using Hsp.Midi.Messages;

namespace Zulweb.MidiPipes;

public class ChannelMessageJsonConverter : JsonConverter<ChannelMessage>
{
  public override ChannelMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    try
    {
      var parts = reader.GetString()?.Split(' ') ?? [];
      var cmd = Enum.Parse<ChannelCommand>(parts[0]);
      var ch = int.Parse(parts[1]);
      var dt1 = byte.Parse(parts[2]);
      var dt2 = byte.Parse(parts[3]);
      return new ChannelMessage(cmd, ch, dt1, dt2);
    }
    catch
    {
      return null;
    }
  }

  public override void Write(Utf8JsonWriter writer, ChannelMessage value, JsonSerializerOptions options)
  {
  }
}