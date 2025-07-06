namespace Zulweb.Editor;

public interface IPage
{
  string Caption { get; }
  bool IsActive { get; }
}