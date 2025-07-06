using Eos.Mvvm;
using Zulweb.Models;

namespace Zulweb.Editor.ViewModels;

public class SetlistItemVm : ObservableEntity
{
  public SetlistItem Model { get; }


  public string? RegionName
  {
    get => Model.RegionName;
    set
    {
      Model.RegionName = value;
      RaisePropertyChanged();
    }
  }

  public bool Disabled
  {
    get => Model.Disabled;
    set
    {
      Model.Disabled = value;
      RaisePropertyChanged();
    }
  }

  public int Sequence
  {
    get => Model.Sequence;
    set
    {
      Model.Sequence = value;
      RaisePropertyChanged();
    }
  }


  public SetlistItemVm(SetlistItem model)
  {
    Model = model;
  }
}