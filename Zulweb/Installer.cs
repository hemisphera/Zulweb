using System.Text.Json;
using Microsoft.Extensions.Options;
using Zulweb.Infrastructure;
using Zulweb.Infrastructure.Settings;
using Zulweb.MidiPipes;
using Zulweb.Models;

namespace Zulweb;

internal static class Installer
{
  public static void AddZulweb(this WebApplicationBuilder builder)
  {
    builder.Services.AddSingleton<SetlistController>();
    builder.Services.AddSingleton<ReaperInterface>();
    builder.Services.AddSingleton<MidiPipeHost>();
    builder.Services.Configure<ReaperSettings>(builder.Configuration.GetSection("Reaper"));
  }

  public static async Task InitializeZulweb(this WebApplication app)
  {
    var reaper = app.Services.GetRequiredService<ReaperInterface>();
    var settings = app.Services.GetRequiredService<IOptions<ReaperSettings>>();
    await reaper.ConnectAsync(settings.Value);

    var sessionSettings = app.Services.GetRequiredService<IOptions<Settings.Session>>().Value;

    if (!string.IsNullOrEmpty(sessionSettings.SetlistPath))
    {
      var controller = app.Services.GetRequiredService<SetlistController>();
      using var fs = File.Open(sessionSettings.SetlistPath, FileMode.Open);
      var setlist = await JsonSerializer.DeserializeAsync<Setlist>(fs) ?? throw new NotSupportedException();
      await controller.Load(setlist);
    }

    if (!string.IsNullOrEmpty(sessionSettings.MidiPipeConfigurationPath))
    {
      var pipeHost = app.Services.GetRequiredService<MidiPipeHost>();
      await pipeHost.Start(sessionSettings.MidiPipeConfigurationPath);
    }
  }
}