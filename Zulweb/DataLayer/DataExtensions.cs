using Microsoft.EntityFrameworkCore;
using Zulweb.Entities;

namespace Zulweb.DataLayer;

internal static class DataExtensions
{
  private const string LastSetlistId = "LastSetlistId";
  private const string LastMidiPipeConfiguration = "LastMidiPipeConfiguration";


  public static async Task<Guid?> GetLastSetlistId(this ZulwebDataContext db)
  {
    var value = await GetValue(db, LastSetlistId);
    return Guid.TryParse(value, out var guid) ? guid : null;
  }

  public static async Task SetLastSetlistId(this ZulwebDataContext db, Guid? id)
  {
    await SetValue(db, LastSetlistId, id == null ? null : $"{id}");
  }

  public static async Task<string?> GetLastMidiPipeConfiguration(this ZulwebDataContext db)
  {
    return await GetValue(db, LastMidiPipeConfiguration);
  }

  public static async Task SetLastMidiPipeConfiguration(this ZulwebDataContext db, string? id)
  {
    await SetValue(db, LastMidiPipeConfiguration, id);
  }


  private static async Task<string?> GetValue(ZulwebDataContext db, string key)
  {
    var row = await db.Settings.FirstOrDefaultAsync(a => a.Key == key);
    return row?.Value;
  }

  private static async Task SetValue(ZulwebDataContext db, string key, string? value)
  {
    var row = db.Settings.FirstOrDefault(a => a.Key == key);

    if (row == null)
    {
      row = new SettingsEntity
      {
        Key = key,
        Value = value
      };
      db.Settings.Add(row);
    }
    else
    {
      row.Value = value;
    }

    await db.SaveChangesAsync();
  }
}