using System.ComponentModel;
using System.Text;

namespace Zulweb.MidiPipes;

internal static class Extensions
{
  public static string GetToken(this string[] arr, int index, string defaultValue = "")
  {
    if (index < 0 || index > arr.Length - 1)
      return defaultValue;
    return arr[index];
  }

  public static int GetIntToken(this string[] arr, int index, int defaultValue = 0)
  {
    var strValue = arr.GetToken(index);
    return int.TryParse(strValue, out var val) ? val : defaultValue;
  }

  public static int? GetIntTokenOrNull(this string[] arr, int index)
  {
    var strValue = arr.GetToken(index);
    if (strValue == "*") return null;
    return int.TryParse(strValue, out var val) ? val : null;
  }

  public static T GetEnumToken<T>(this string[] arr, int index) where T : struct
  {
    var strValue = arr.GetToken(index);
    return Enum.Parse<T>(strValue);
  }

  public static T? GetEnumTokenOrNull<T>(this string[] arr, int index) where T : struct
  {
    var strValue = arr.GetToken(index);
    if (strValue == "*") return null;
    return Enum.TryParse<T>(strValue, out var val) ? val : null;
  }

  public static T[]? GetEnumTokenMultiple<T>(this string[] arr, int index) where T : struct
  {
    var strVal = GetToken(arr, index);
    if (string.IsNullOrEmpty(strVal) || strVal == "*") return null;
    var strValues = strVal.Split('|');
    return strValues
      .Select(v => Enum.TryParse<T>(v, out var enumValue) ? (T?)enumValue : null)
      .OfType<T>().ToArray();
  }

  public static Range? GetRangeToken(this string[] arr, int index)
  {
    var strVal = GetToken(arr, index);
    if (string.IsNullOrEmpty(strVal) || strVal == "*") return null;
    return Range.TryParse(strVal, out var parsedValue) ? parsedValue : null;
  }

  public static Range[]? GetRangeTokenMultiple(this string[] arr, int index)
  {
    var strVal = GetToken(arr, index);
    if (string.IsNullOrEmpty(strVal) || strVal == "*") return null;
    var strValues = strVal.Split('|');
    return strValues
      .Select(v => Range.TryParse(v, out var parsedValue) ? parsedValue : null)
      .OfType<Range>().ToArray();
  }

  public static string[] EnsureLength(this string[] arr, int length, string filler = "")
  {
    return arr.Length >= length
      ? arr
      : arr.Concat(Enumerable.Repeat(filler, length - arr.Length)).ToArray();
  }

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