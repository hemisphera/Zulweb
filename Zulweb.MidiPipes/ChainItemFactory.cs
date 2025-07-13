using System.Collections.Concurrent;
using Zulweb.MidiPipes.Chains;

namespace Zulweb.MidiPipes;

internal static class ChainItemFactory
{
  private static readonly ConcurrentDictionary<string, Type> Items = [];


  public static IMidiChainItem CreateItem(string name)
  {
    var type = FindChainItemType(name);
    return Activator.CreateInstance(type) as IMidiChainItem ?? throw new NotSupportedException();
  }

  private static Type FindChainItemType(string name)
  {
    return Items.GetOrAdd(name.ToLowerInvariant(), LoadTypeByName);
  }

  private static Type LoadTypeByName(string arg)
  {
    var allTypes = typeof(IMidiChainItem).Assembly
      .GetTypes()
      .Where(t => t.GetInterfaces().Contains(typeof(IMidiChainItem)))
      .ToArray();
    var foundType = allTypes.FirstOrDefault(t => t.Name.Equals(arg + "ChainItem", StringComparison.OrdinalIgnoreCase));
    if (foundType == null)
      throw new Exception($"Unable to find type for {arg}.");
    return foundType;
  }
}