using Zulweb.Entities;
using Zulweb.Models;

namespace Zulweb.DataLayer;

internal static class ModelMappers
{
  public static Setlist ToModel(this SetlistEntity entity)
  {
    return new Setlist
    {
      Id = entity.Id,
      Name = entity.Name,
      Description = entity.Description
    };
  }

  public static SetlistItem ToModel(this SetlistItemEntity entity)
  {
    return new SetlistItem
    {
      RegionName = entity.RegionName,
      Disabled = entity.Disabled,
      Sequence = entity.Sequence
    };
  }
}