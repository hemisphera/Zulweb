using System.Text;

namespace Zulweb.MidiPipes;

internal static class Extensions
{
  public static string[] SplitWithDelimiter(this string str, char delimiter = ' ', char quoteChar = '\'')
  {
    var result = new List<string>();
    var sb = new StringBuilder();
    var inQuotes = false;
    foreach (var ch in str)
    {
      if (ch == quoteChar)
      {
        inQuotes = !inQuotes;
        continue;
      }

      if (ch != delimiter || inQuotes)
      {
        sb.Append(ch);
        continue;
      }

      if (ch == delimiter && !inQuotes)
      {
        result.Add(sb.ToString());
        sb.Clear();
      }
    }

    if (sb.Length > 0) result.Add(sb.ToString());
    return result.ToArray();
  }
}