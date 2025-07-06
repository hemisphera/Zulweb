using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Zulweb.Editor.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView : Window
{
  public MainView()
  {
    InitializeComponent();
    DataContext = App.Services.GetRequiredService<Main>();
  }
}