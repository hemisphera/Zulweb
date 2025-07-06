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
    builder.Services.AddScoped<ISetlistStorage, JsonSetlistStorage>();
  }

  public static async Task InitializeZulweb(this WebApplication app)
  {
    var reaper = app.Services.GetRequiredService<ReaperInterface>();
    var settings = app.Services.GetRequiredService<IOptions<ReaperSettings>>();
    await reaper.ConnectAsync(settings.Value);

    var setlistName = GlobalState.LastSetlistName;
    if (!string.IsNullOrEmpty(setlistName))
    {
      var storage = app.Services.GetRequiredService<ISetlistStorage>();
      var controller = app.Services.GetRequiredService<SetlistController>();
      await controller.Load(await storage.Load(setlistName));
    }
  }
}