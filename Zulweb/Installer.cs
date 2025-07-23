using System.Text.Json;
using Microsoft.Extensions.Options;
using Zulweb.Infrastructure;
using Zulweb.LedProxy;
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
    builder.Services.AddTransient<IPackageSender, UdpPackageSender>();

    builder.Services.Configure<Settings.Reaper>(builder.Configuration.GetSection(nameof(Settings.Reaper)));
    builder.Services.Configure<Settings.Setlist>(builder.Configuration.GetSection(nameof(Settings.Setlist)));
    builder.Services.Configure<Settings.MidiPipes>(builder.Configuration.GetSection(nameof(Settings.MidiPipes)));
    builder.Services.Configure<LedStripSettings>(builder.Configuration.GetSection(nameof(LedStripSettings)));
    builder.Services.Configure<Settings.AbletonLink>(builder.Configuration.GetSection(nameof(Settings.AbletonLink)));

    builder.Services.AddHostedService<LedStripDispatcher>();
    builder.Services.AddHostedService<ClockProvider>();
  }

  public static async Task InitializeZulweb(this WebApplication app)
  {
    var reaper = app.Services.GetRequiredService<ReaperInterface>();
    var settings = app.Services.GetRequiredService<IOptions<Settings.Reaper>>();
    await reaper.ConnectAsync(settings.Value);

    var setlistSettings = app.Services.GetRequiredService<IOptions<Settings.Setlist>>().Value;
    if (!string.IsNullOrEmpty(setlistSettings.FilePath))
    {
      var controller = app.Services.GetRequiredService<SetlistController>();
      using var fs = File.Open(setlistSettings.FilePath, FileMode.Open);
      var setlist = await JsonSerializer.DeserializeAsync<Setlist>(fs) ?? throw new NotSupportedException();
      await controller.Load(setlist);
    }

    var midiPipeSettings = app.Services.GetRequiredService<IOptions<Settings.MidiPipes>>().Value;
    if (!string.IsNullOrEmpty(midiPipeSettings.FilePath))
    {
      var pipeHost = app.Services.GetRequiredService<MidiPipeHost>();
      await pipeHost.Start(midiPipeSettings.FilePath);
    }
  }
}