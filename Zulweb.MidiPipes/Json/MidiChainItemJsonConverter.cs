using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Hsp.Midi;
using Zulweb.MidiPipes.Chains;

namespace Zulweb.MidiPipes;

public class MidiChainItemJsonConverter : JsonConverter<IMidiChainItem[]>
{
  public override IMidiChainItem[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    if (JsonNode.Parse(ref reader) is not JsonArray arr) throw new NotSupportedException();
    var result = new List<IMidiChainItem?>();
    foreach (var token in arr)
    {
      IMidiChainItem? item = null;
      if (token is JsonValue jv && jv.GetValueKind() == JsonValueKind.String)
      {
        item = ReadChainItemFromString(jv.GetValue<string>());
      }

      if (item != null)
      {
        result.Add(item);
      }
    }

    return result.OfType<IMidiChainItem>().ToArray();
  }

  private static IMidiChainItem? ReadChainItemFromString(string str)
  {
    var parts = str.SplitWithDelimiter();
    var item = ChainItemFactory.CreateItem(parts[0]);
    item.FromString(parts.Skip(1).ToArray());
    return item;
    /*
    switch (type)
    {
      case ChainItemType.Modify:
        return new ModifyChainItem
        {
          Type = Enum.Parse<ValueType>(parts[1]),
          Expression = parts[2],
          MinValue = parts.Length >= 4 ? int.Parse(parts[3]) : 0,
          MaxValue = parts.Length >= 5 ? int.Parse(parts[4]) : 127
        };
    }
    */
    return null;
  }

  public override void Write(Utf8JsonWriter writer, IMidiChainItem[] value, JsonSerializerOptions options)
  {
    throw new NotSupportedException();
  }
}