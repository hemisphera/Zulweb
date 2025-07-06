using Eos.Mvvm;
using Eos.Mvvm.Attributes;
using Eos.Mvvm.Commands;
using Zulweb.Editor.ApiClient;

namespace Zulweb.Editor.Views;

public class Connection : ObservableEntity
{
  public string Hostname
  {
    get => GetAutoFieldValue<string>();
    set => SetAutoFieldValue(value);
  }

  public bool Connected
  {
    get => GetAutoFieldValue<bool>();
    set => SetAutoFieldValue(value);
  }


  public UiCommand DisconnectCommand { get; }

  public UiCommand ConnectCommand { get; }


  public Connection()
  {
    Hostname = "http://localhost:5000";
    ConnectCommand = MethodUiCommand.FromMethod(this, nameof(Connect));
    DisconnectCommand = MethodUiCommand.FromMethod(this, nameof(Disconnect));
  }


  [UiCommand(Caption = "Connect")]
  public async Task Connect()
  {
    var cl = GetClient();
    await cl.List();
    Connected = true;
  }

  [UiCommand(Caption = "Disconnect")]
  public async Task Disconnect()
  {
    Connected = false;
    await Task.CompletedTask;
  }

  public SetlistApiClient GetClient()
  {
    return new SetlistApiClient(Hostname);
  }
}