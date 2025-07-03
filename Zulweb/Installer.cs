using Microsoft.Extensions.Options;
using Syncfusion.Blazor;
using Zulweb.Infrastructure;
using Zulweb.Infrastructure.Settings;

namespace Zulweb;

internal static class Installer
{
  public static void AddZulweb(this WebApplicationBuilder builder)
  {
    //builder.Services.AddTransient<ISyncfusionStringLocalizer, SyncfusionStringLocalizer>();
    builder.Services.AddSyncfusionBlazor();
    
    builder.Services.AddSingleton<SetlistController>();
    builder.Services.AddSingleton<ReaperInterface>();
    builder.Services.Configure<ReaperSettings>(builder.Configuration.GetSection("Reaper"));
    builder.Services.Configure<SetlistSettings>(builder.Configuration.GetSection("Setlist"));
  }

  public static async Task InitializeZulweb(this WebApplication app)
  {
    var reaper = app.Services.GetRequiredService<ReaperInterface>();
    var settings = app.Services.GetRequiredService<IOptions<ReaperSettings>>();
    await reaper.ConnectAsync(settings.Value);

    var setlist = app.Services.GetRequiredService<IOptions<SetlistSettings>>();
    var path = setlist.Value.DefaultSetlistPath;
    if (!string.IsNullOrEmpty(path))
    {
      var setup = app.Services.GetRequiredService<SetlistController>();
      setup.LoadFromFile(path);
    }
  }
}