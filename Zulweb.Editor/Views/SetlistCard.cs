using Eos.Mvvm;
using Zulweb.Editor.ViewModels;

namespace Zulweb.Editor.Views;

public class SetlistCard : AsyncItemsViewModelBase<SetlistItemVm>, IPage
{
  public SetlistVm Setlist { get; }

  public string Caption => "Setlist";

  public bool IsActive
  {
    get => GetAutoFieldValue<bool>();
    set => SetAutoFieldValue(value);
  }


  public SetlistCard(SetlistVm setlist)
  {
    Setlist = setlist;
  }


  protected override async Task<IEnumerable<SetlistItemVm>> GetItems()
  {
    return await Task.FromResult(Setlist.Items);
  }
}