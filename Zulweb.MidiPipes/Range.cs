using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Zulweb.MidiPipes;

public class Range
{
  private static readonly Regex RangeRegex = new(@"^(?<min>\d+)(?:\.\.(?<max>\d+))?$");


  public static Range Parse(string str)
  {
    return TryParse(str, out var range) ? range : throw new FormatException();
  }

  public static bool TryParse(string str, [NotNullWhen(true)] out Range? range)
  {
    range = null;
    var m = RangeRegex.Match(str);
    if (!m.Success) return false;
    var min = int.Parse(m.Groups["min"].Value);
    var max = m.Groups["max"].Success ? int.Parse(m.Groups["max"].Value) : (int?)null;
    range = new Range(min, max);
    return true;
  }


  public int Minimum { get; set; }

  public int? Maximum { get; set; }


  public Range(int minimum, int? maximum = null)
  {
    Minimum = minimum;
    Maximum = maximum;
  }


  public bool Contains(int value)
  {
    return Maximum == null
      ? value.Equals(Minimum)
      : value.CompareTo(Minimum) >= 0 && value.CompareTo(Maximum.Value) <= 0;
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