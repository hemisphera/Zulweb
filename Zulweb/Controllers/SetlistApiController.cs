using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zulweb.DataLayer;
using Zulweb.Infrastructure;
using Zulweb.Models;

namespace Zulweb.Controllers;

[ApiController]
[Route("~/api/setlist")]
public class SetlistApiController : ControllerBase
{
  private readonly SetlistController _setlistController;
  private readonly ZulwebDataContext _db;


  public SetlistApiController(SetlistController setlistController, ZulwebDataContext db)
  {
    _setlistController = setlistController;
    _db = db;
  }


  [HttpGet]
  [Route("{id:guid}")]
  public async Task<IActionResult> GetSetlist([FromRoute] Guid id)
  {
    var setlist = await _db.LoadSetlist(id);
    return Ok(setlist);
  }

  [HttpGet]
  public async Task<IActionResult> ListSetlists()
  {
    var ids = await _db.Setlists.Select(sl => sl.Id).ToArrayAsync();
    var setLists = await Task.WhenAll(ids.Select(id => _db.LoadSetlist(id)));
    return Ok(setLists.OrderBy(s => s.Name));
  }

  [HttpDelete]
  [Route("{id:guid}")]
  public async Task<IActionResult> DeleteSetlist([FromRoute] Guid id)
  {
    await _db.Setlists.Where(s => s.Id == id).ExecuteDeleteAsync();
    await _db.SetlistItems.Where(s => s.SetlistId == id).ExecuteDeleteAsync();
    return Accepted();
  }

  [HttpPost]
  [Route("current/{id:guid}")]
  public async Task<IActionResult> LoadSetlist([FromRoute] Guid id)
  {
    var setlist = await _db.LoadSetlist(id);
    await _setlistController.Load(setlist);
    return Accepted();
  }

  [HttpPost]
  public async Task<IActionResult> Upload([FromBody] Setlist setlist)
  {
    //await _storage.Save(setlist);
    return Accepted();
  }
}