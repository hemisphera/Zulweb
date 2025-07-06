using Microsoft.AspNetCore.Mvc;
using Zulweb.Infrastructure;
using Zulweb.Models;

namespace Zulweb.Controllers;

[ApiController]
[Route("~/api/setlist")]
public class SetlistApiController : ControllerBase
{
  private readonly SetlistController _setlistController;
  private readonly ISetlistStorage _storage;


  public SetlistApiController(SetlistController setlistController, ISetlistStorage storage)
  {
    _setlistController = setlistController;
    _storage = storage;
  }


  [HttpGet]
  [Route("{name}")]
  public async Task<IActionResult> GetSetlist([FromRoute] string name)
  {
    var setlist = await _storage.Load(name);
    return Ok(setlist);
  }

  [HttpGet]
  public async Task<IActionResult> ListSetlists()
  {
    var setlists = await _storage.List();
    return Ok(setlists);
  }

  [HttpDelete]
  [Route("{name}")]
  public async Task<IActionResult> DeleteSetlist([FromRoute] string name)
  {
    await _storage.Delete(name);
    return Accepted();
  }

  [HttpPost]
  [Route("{name}/load")]
  public async Task<IActionResult> LoadSetlist([FromRoute] string name)
  {
    var setlist = await _storage.Load(name);
    await _setlistController.Load(setlist);
    return Accepted();
  }

  [HttpPost]
  [Route("{name}")]
  public async Task<IActionResult> Upload([FromRoute] string name, [FromBody] Setlist setlist)
  {
    await _storage.Save(name, setlist);
    return Accepted();
  }
}