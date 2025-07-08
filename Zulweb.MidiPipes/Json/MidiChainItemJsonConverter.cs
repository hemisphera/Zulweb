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
      if (token is JsonObject jObj)
      {
        item = ReadChainItemFromObject(jObj, options);
      }

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

  private static IMidiChainItem? ReadChainItemFromObject(JsonObject obj, JsonSerializerOptions options)
  {
    if (!Enum.TryParse<ChainItemType>(obj["Type"]?.GetValue<string>(), true, out var type))
    {
      return null;
    }

    switch (type)
    {
      case ChainItemType.Delay:
        return obj.Deserialize<DelayMidiChainItem>(options);
      case ChainItemType.Output:
        return obj.Deserialize<OutputMidiChainItem>(options);
      case ChainItemType.NoteToController:
        return obj.Deserialize<NoteToControllerChainItem>(options);
      case ChainItemType.NoteToProgramChange:
        return obj.Deserialize<NoteToProgramChangeChainItem>(options);
      case ChainItemType.Filter:
        return obj.Deserialize<FilterChainItem>(options);
      case ChainItemType.Velocity:
        return obj.Deserialize<VelocityChainItem>(options);
      case ChainItemType.Fork:
        return obj.Deserialize<ForkChainItem>(options);
      case ChainItemType.Dump:
        return obj.Deserialize<DumpChainItem>(options);
      case ChainItemType.Message:
        return obj.Deserialize<MessageMidiChainItem>(options);
      case ChainItemType.Read:
        return obj.Deserialize<ReadValueChainItem>(options);
      case ChainItemType.Write:
        return obj.Deserialize<WriteValueChainItem>(options);
      case ChainItemType.Modify:
        return obj.Deserialize<ModifyChainItem>(options);
    }

    return null;
  }

  private static IMidiChainItem? ReadChainItemFromString(string str)
  {
    var parts = str.SplitWithDelimiter();
    if (!Enum.TryParse<ChainItemType>(parts[0], true, out var type))
    {
      return null;
    }

    switch (type)
    {
      case ChainItemType.Dump:
        return new DumpChainItem();
      case ChainItemType.NoteToProgramChange:
        return new NoteToProgramChangeChainItem
        {
          Value = Enum.Parse<NoteToCcValueType>(parts[1]),
          Channel = parts[2] == "*" ? null : int.Parse(parts[2])
        };
      case ChainItemType.NoteToController:
        return new NoteToControllerChainItem
        {
          Value = Enum.Parse<NoteToCcValueType>(parts[1]),
          ControllerNumber = int.Parse(parts[2])
        };
      case ChainItemType.Filter:
        return FilterChainItem.FromString(parts);
      case ChainItemType.Delay:
        return new DelayMidiChainItem
        {
          Milliseconds = int.Parse(parts[1])
        };
      case ChainItemType.Output:
        return new OutputMidiChainItem
        {
          PortName = parts.Length > 1 ? parts[1] : null
        };
      case ChainItemType.Message:
        return new MessageMidiChainItem
        {
          Command = parts[1] == "*" ? null : Enum.Parse<ChannelCommand>(parts[1]),
          Channel = parts[2] == "*" ? null : int.Parse(parts[2]),
          Data1 = parts[3] == "*" ? null : byte.Parse(parts[3]),
          Data2 = parts[4] == "*" ? null : byte.Parse(parts[4]),
        };
      case ChainItemType.Read:
        return new ReadValueChainItem
        {
          VariableName = parts[1],
          Type = Enum.Parse<ValueType>(parts[2]),
          InitValue = byte.Parse(parts[3])
        };
      case ChainItemType.Write:
        return new WriteValueChainItem
        {
          VariableName = parts[1],
          Type = Enum.Parse<ValueType>(parts[2])
        };
      case ChainItemType.Modify:
        return new ModifyChainItem
        {
          Type = Enum.Parse<ValueType>(parts[1]),
          Expression = parts[2],
          MinValue = parts.Length >= 4 ? int.Parse(parts[3]) : 0,
          MaxValue = parts.Length >= 5 ? int.Parse(parts[4]) : 127
        };
    }

    return null;
  }

  public override void Write(Utf8JsonWriter writer, IMidiChainItem[] value, JsonSerializerOptions options)
  {
    throw new NotSupportedException();
  }
}