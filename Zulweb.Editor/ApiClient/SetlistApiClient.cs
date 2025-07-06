using System.Net.Http;
using Zulweb.Models;

namespace Zulweb.Editor.ApiClient;

public class SetlistApiClient
{
  private readonly string _url;


  public SetlistApiClient()
    : this("http://localhost:5000")
  {
  }

  public SetlistApiClient(string url)
  {
    _url = url;
  }


  public async Task<string[]> List()
  {
    return await RestClient.Create()
      .Method(HttpMethod.Get)
      .Uri($"{_url}/api/setlist")
      .GetJsonResponse<string[]>();
  }

  public async Task<Setlist> Download(string name)
  {
    return await RestClient.Create()
      .Method(HttpMethod.Get)
      .Uri($"{_url}/api/setlist/{name}")
      .GetJsonResponse<Setlist>();
  }

  public async Task Upload(Setlist item)
  {
    await RestClient.Create()
      .Method(HttpMethod.Post)
      .JsonPayload(item)
      .Uri($"{_url}/api/setlist/{item.Name}")
      .Send();
  }

  public async Task Delete(Setlist item)
  {
    await RestClient.Create()
      .Method(HttpMethod.Delete)
      .Uri($"{_url}/api/setlist/{item.Name}")
      .Send();
  }
}