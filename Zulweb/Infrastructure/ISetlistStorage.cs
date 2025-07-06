using Zulweb.Models;

namespace Zulweb.Infrastructure;

public interface ISetlistStorage
{
  Task<string[]> List();
  Task Delete(string name);
  Task<Setlist> Load(string name);
  Task Save(string name, Setlist item);
}