using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Zulweb.MidiPipes.Chains;

namespace Zulweb.MidiPipes;

public class Range
{
  private static readonly Regex RangeRegex1 = new(@"^(?<prefix>[\\>\\<\\!=]+)?(?<min>\d+)(?:\.\.(?<max>\d+))?$");


  public static Range Parse(string str)
  {
    return TryParse(str, out var range) ? range : throw new FormatException();
  }

  public static bool TryParse(string str, [NotNullWhen(true)] out Range? range)
  {
    range = null;
    var m = RangeRegex1.Match(str);
    if (!m.Success) return false;

    var min = int.Parse(m.Groups["min"].Value);
    var max = m.Groups["max"].Success ? int.Parse(m.Groups["max"].Value) : (int?)null;
    range = new Range(min, max)
    {
      Operator = ParseOperator(m.Groups["prefix"].Value)
    };
    return true;
  }

  private static RangeOperator ParseOperator(string value)
  {
    return value switch
    {
      "<>" => RangeOperator.NotEqual,
      ">" => RangeOperator.GreaterThan,
      ">=" => RangeOperator.GreaterThanOrEqual,
      "<" => RangeOperator.LesserThan,
      "<=" => RangeOperator.LesserThanOrEqual,
      _ => RangeOperator.Equal
    };
  }

  public int Minimum { get; set; }

  public int? Maximum { get; set; }

  public RangeOperator Operator { get; set; }


  public Range(int minimum, int? maximum = null)
  {
    Minimum = minimum;
    Maximum = maximum;
  }


  public bool Matches(int value)
  {
    var matches = Maximum == null
      ? value.Equals(Minimum)
      : value.CompareTo(Minimum) >= 0 && value.CompareTo(Maximum.Value) <= 0;

    if (Operator == RangeOperator.Equal)
      return matches;
    if (Operator == RangeOperator.NotEqual)
      return !matches;

    if (Operator == RangeOperator.GreaterThan) return value > Minimum;
    if (Operator == RangeOperator.GreaterThanOrEqual) return value >= Minimum;
    if (Operator == RangeOperator.LesserThan) return value < Minimum;
    if (Operator == RangeOperator.LesserThanOrEqual) return value <= Minimum;
    return false;
  }

  public int Limit(int value)
  {
    if (value < Minimum) return Minimum;
    return value > Maximum ? Maximum.Value : value;
  }

  public override string ToString()
  {
    var result = $"{Minimum}";
    if (Maximum != null)
      result = $"{result}..{Maximum}";
    return result;
  }
}