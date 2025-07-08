using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Zulweb.Entities;
using Zulweb.Models;

namespace Zulweb.DataLayer;

public class ZulwebDataContext : DbContext
{
  private string _dbPath;

  public DbSet<SetlistEntity> Setlists { get; set; }
  public DbSet<SetlistItemEntity> SetlistItems { get; set; }
  public DbSet<SettingsEntity> Settings { get; set; }


  public ZulwebDataContext()
  {
    _dbPath = Path.Combine(Zulweb.Infrastructure.Globals.RootFolder, "database.sqlite");
  }


  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseSqlite($"Data Source={_dbPath}");
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<SetlistEntity>()
      .Property(a => a.Id).HasConversion(a => a.ToString("D"), a => Guid.Parse(a));
    modelBuilder.Entity<SetlistItemEntity>()
      .Property(a => a.SetlistId).HasConversion(a => a.ToString("D"), a => Guid.Parse(a));
  }


  public async Task<Setlist> LoadSetlist(Guid id)
  {
    var all = await Setlists.ToArrayAsync();
    var header = await Setlists.FirstAsync(sl => sl.Id == id);
    var result = header.ToModel();

    var lines = await SetlistItems.Where(l => l.SetlistId == header.Id).ToListAsync();
    result.Items = lines.Select(l => l.ToModel()).ToArray();

    return result;
  }
}