using System.Text.RegularExpressions;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;

namespace Zulweb.MidiPipes.Chains;

/// <summary>
/// Modifies the value of a MIDI message.
/// </summary>
public class ModifyChainItem : IMidiChainItem
{
  public ValueType Type { get; set; }

  public int MaxValue { get; set; } = 127;

  public int MinValue { get; set; } = 0;

  public string Expression { get; set; } = string.Empty;


  public async Task ProcessAsync(Connection connection, IMidiMessage message, Func<IMidiMessage, Task> next)
  {
    if (message is not ChannelMessage cm) return;
    switch (Type)
    {
      case ValueType.Channel:
        cm.Channel = Apply(cm.Channel);
        break;
      case ValueType.Data1:
        cm.Data1 = Apply(cm.Data1);
        break;
      case ValueType.Data2:
        cm.Data2 = Apply(cm.Data2);
        break;
    }

    await next(cm);
  }

  private int Apply(int val)
  {
    if (string.IsNullOrEmpty(Expression)) return val;
    var m = Regex.Match(Expression, "^([+-\\/\\*]?)(?<op>\\d+)$");
    if (!m.Success) return val;
    var op = Expression[0];
    var opVal = int.Parse(m.Groups["op"].Value);
    var result = op switch
    {
      '+' => val + opVal,
      '-' => val - opVal,
      '*' => val * opVal,
      '/' => val / opVal,
      _ => val
    };
    return int.Clamp(result, MinValue, MaxValue);
  }


  public Task Initialize(Connection connection, ILogger? logger = null)
  {
    return Task.CompletedTask;
  }

  public Task Deinitialize()
  {
    return Task.CompletedTask;
  }

  /// <summary>
  /// Parameters:
  /// [0]: The type of value to modify. Can be "Command", "Channel", "Data1", "Data2"
  /// [1]: modification expression. +1, -4, *2, /6 etc.
  /// [2]: A minimum value. After applying the expression, values below this will truncate to this. Defaults to 0.
  /// [3]: A maximum value. After applying the expression, values above this will truncate to this. Defaults to 127.
  /// </summary>
  /// <param name="tokens"></param>
  public void FromString(string[] tokens)
  {
    Type = tokens.GetEnumToken<ValueType>(0);
    Expression = tokens.GetToken(1);
    MinValue = tokens.GetIntToken(2);
    MaxValue = tokens.GetIntToken(3, 127);
  }
}