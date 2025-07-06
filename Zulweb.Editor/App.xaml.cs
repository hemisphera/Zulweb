using System.Reflection;
using Eos.Mvvm;
using Eos.Mvvm.DataTemplates;
using Eos.Mvvm.EventArgs;
using Application = System.Windows.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zulweb.Editor.ApiClient;
using Zulweb.Editor.Views;

namespace Zulweb.Editor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  private static IHost? _appHost;
  public static IServiceProvider Services => _appHost!.Services;


  public App()
  {
    UiSettings.ViewLocator = new ViewLocator(typeof(App).Assembly);
    ConfigureHost();
  }


  private static void ConfigureHost()
  {
    var hb = Host.CreateApplicationBuilder();
    hb.Services.AddSingleton<Main>();
    hb.Services.AddSingleton<Sidebar>();
    hb.Services.AddSingleton<Connection>();
    hb.Services.AddSingleton<SetlistList>();
    hb.Services.AddTransient<SetlistApiClient>();

    /*
    hb.Services.AddLogging(loggingbuilder =>
    {
      loggingbuilder.ClearProviders();
      loggingbuilder.AddFile(GlobalConfiguration.LogFilepath, options =>
      {
        options.Append = true;
        options.FileSizeLimitBytes = 10_000_000;
      });
    });
    hb.Logging.AddGordonTelemetry(new GordonTelemetryProperties(Assembly.GetExecutingAssembly()));
    */
    _appHost = hb.Build();
  }

  private void InverseBooleanConverter_OnOnConvert(object? sender, ConverterEventArgs e)
  {
    e.Result = !(bool)e.Value;
  }
}