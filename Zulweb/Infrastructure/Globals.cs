namespace Zulweb.Infrastructure;

internal static class Globals
{
  public static string RootFolder { get; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Zulweb");
}