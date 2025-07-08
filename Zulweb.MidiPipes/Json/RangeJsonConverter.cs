using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zulweb.MidiPipes;

public class RangeJsonConverter : JsonConverter<Range>
{
  public override Range? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var str = reader.GetString();
    if (string.IsNullOrEmpty(str)) return null;
    return !Range.TryParse(str, out var range) ? null : range;
  }

  public override void Write(Utf8JsonWriter writer, Range value, JsonSerializerOptions options)
  {
    writer.WriteRawValue(JsonEncodedText.Encode(value.ToString()).Value);
  }
}