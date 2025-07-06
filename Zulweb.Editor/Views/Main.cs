using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace Zulweb.Editor.Views;

public class Main
{
  public ObservableCollection<IPage> Pages { get; } = [];

  public Main()
  {
    Pages.Add(App.Services.GetRequiredService<Sidebar>());
  }
}