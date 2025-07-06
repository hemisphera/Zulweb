using Eos.Mvvm;

namespace Zulweb.Editor.Views;

public class Sidebar : ObservableEntity, IPage
{
  public string Caption => "Setlist";

  public bool IsActive
  {
    get => GetAutoFieldValue<bool>();
    set => SetAutoFieldValue(value);
  }

  public Connection Connection { get; }
  public SetlistList List { get; }


  public Sidebar(Connection conn, SetlistList list)
  {
    Connection = conn;
    List = list;
  }
}