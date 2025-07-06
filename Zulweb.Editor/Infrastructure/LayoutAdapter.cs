using DevExpress.Xpf.Docking;
using Zulweb.Editor.Views;

namespace Zulweb.Editor.Infrastructure;

public class LayoutAdapter : ILayoutAdapter
{
  public string Resolve(DockLayoutManager owner, object item)
  {
    return item is Sidebar
      ? "Panel"
      : "Documents";
  }
}