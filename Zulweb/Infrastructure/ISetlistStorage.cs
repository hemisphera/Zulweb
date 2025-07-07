using Zulweb.Models;

namespace Zulweb.Infrastructure;

public interface ISetlistStorage
{
  IAsyncEnumerable<Setlist> List();
  Task Delete(string name);
  Task Save(Setlist item);


  public async Task<Setlist> Get(string name)
  {
    var setlists = await List().ToArrayAsync();
    return setlists.First(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
  }

  public async Task<Setlist> GetById(Guid id)
  {
    var setlists = await List().ToArrayAsync();
    return setlists.First(a => a.Id == id);
  }
}