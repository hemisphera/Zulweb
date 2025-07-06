using Eos.Mvvm;
using Eos.Mvvm.Commands;
using Zulweb.Editor.ApiClient;
using Zulweb.Models;

namespace Zulweb.Editor.Views;

public class SetlistList : AsyncItemsViewModelBase<Setlist>, IPage
{
  private readonly Connection _conn;
  public string Caption => "Setlist";

  public bool IsActive
  {
    get => GetAutoFieldValue<bool>();
    set => SetAutoFieldValue(value);
  }


  public UiCommand AddCommand { get; }
  public UiCommand DeleteCommand { get; }


  public SetlistList(Connection conn)
  {
    _conn = conn;
    AddCommand = MethodUiCommand.FromMethod(this, nameof(AddSetlist));
    DeleteCommand = MethodUiCommand.FromMethod(this, nameof(DeleteSetlist));
  }


  protected override async Task<IEnumerable<Setlist>> GetItems()
  {
    var cl = new SetlistApiClient();
    var names = await cl.List();
    var items = await Task.WhenAll(names.Select(name => cl.Download(name)));
    return items.OrderBy(i => i.Name);
  }


  public async Task AddSetlist()
  {
    var cl = _conn.GetClient();
    await cl.Upload(new Setlist
    {
      Name = "new-setlist"
    });
    await Refresh();
  }

  public async Task DeleteSetlist()
  {
    if (SelectedItem == null) return;
    var cl = _conn.GetClient();
    await cl.Delete(SelectedItem);
    await Refresh();
  }
}