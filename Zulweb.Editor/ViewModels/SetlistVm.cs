using Eos.Mvvm;
using Zulweb.Models;

namespace Zulweb.Editor.ViewModels;

public class SetlistVm : ObservableEntity
{
  private readonly Setlist _model;


  public string Name
  {
    get => _model.Name;
    set
    {
      _model.Name = value;
      RaisePropertyChanged();
    }
  }

  public string Description
  {
    get => _model.Description;
    set
    {
      _model.Description = value;
      RaisePropertyChanged();
    }
  }

  public MappedObservableCollection<SetlistItem, SetlistItemVm> Items { get; }


  public SetlistVm(Setlist model)
  {
    _model = model;
    Items = new MappedObservableCollection<SetlistItem, SetlistItemVm>(
      _model.Items,
      source => new SetlistItemVm(source),
      target => target.Model
    );
  }
}