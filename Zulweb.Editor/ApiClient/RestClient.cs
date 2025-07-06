using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Xml;

namespace Zulweb.Editor.ApiClient;

internal class RestClient : IDisposable
{
  private readonly HttpClient _client;
  private readonly HttpRequestMessage _request;
  private readonly bool _canDisposeClient;


  public static RestClient Create(HttpClient? client = null)
  {
    return new RestClient(client);
  }

  private RestClient(HttpClient? client = null)
  {
    _request = new HttpRequestMessage();
    _client = client ?? new HttpClient();
    _canDisposeClient = client == null;
  }


  public RestClient Method(HttpMethod method)
  {
    _request.Method = method;
    return this;
  }

  public RestClient Uri(string uri)
  {
    return Uri(new Uri(uri));
  }

  public RestClient Uri(Uri uri)
  {
    _request.RequestUri = uri;
    return this;
  }

  public RestClient JsonPayload(object payload)
  {
    _request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    return this;
  }

  public RestClient StreamPayload(Stream stream)
  {
    _request.Content = new StreamContent(stream);
    return this;
  }

  public RestClient Header(string name, string value)
  {
    _request.Headers.Add(name, value);
    return this;
  }

  public RestClient HeaderIfNotNullOrEmpty(string name, string value)
  {
    return string.IsNullOrEmpty(value) ? this : Header(name, value);
  }

  public RestClient QueryParam(string name, DateTime? date)
  {
    return date == null ? this : QueryParam(name, XmlConvert.ToString(date.Value, XmlDateTimeSerializationMode.Local));
  }

  public RestClient QueryParam(string name, bool? value)
  {
    if (value == null) return this;
    QueryParam(name, $"{(value == true ? "true" : "false")}");
    return this;
  }

  public RestClient QueryParam(string name, string value)
  {
    if (string.IsNullOrEmpty(value)) return this;

    var ub = new UriBuilder(_request.RequestUri);
    var p = HttpUtility.ParseQueryString(ub.Query);
    p.Set(name, value);
    ub.Query = p.ToString();
    _request.RequestUri = ub.Uri;

    return this;
  }

  public RestClient QueryParamIfNotNull(string name, bool? value)
  {
    return value == null ? this : QueryParam(name, value.Value);
  }

  public RestClient QueryParamIf(bool condition, string name, string value)
  {
    return !condition ? this : QueryParam(name, value);
  }

  public RestClient QueryParamIf(bool condition, string name, bool value)
  {
    return !condition ? this : QueryParam(name, value);
  }


  private async Task<HttpResponseMessage> GetResponse(CancellationToken ct = default)
  {
    var response = await _client.SendAsync(_request, ct);
    return response;
  }


  public async Task<T> GetJsonResponse<T>(CancellationToken ct = default)
  {
    var response = await GetResponse(ct);
    var responseStr = await response.Content.ReadAsStringAsync(ct);
    if (string.IsNullOrEmpty(responseStr))
      responseStr = $"{response.StatusCode}";
    if (!response.IsSuccessStatusCode)
      throw new Exception($"{response.StatusCode} {responseStr}");
    return JsonSerializer.Deserialize<T>(responseStr) ?? throw new Exception();
  }

  public async Task<Stream> GetBinaryResponse()
  {
    var response = await GetResponse();
    if (response.IsSuccessStatusCode)
    {
      return await response.Content.ReadAsStreamAsync();
    }

    var responseStr = await response.Content.ReadAsStringAsync();
    throw new Exception($"{response.StatusCode} {responseStr}");
  }

  public async Task Send(CancellationToken ct = default)
  {
    var response = await GetResponse(ct);
    var responseStr = await response.Content.ReadAsStringAsync(ct);
    if (!response.IsSuccessStatusCode)
      throw new Exception($"{response.StatusCode} {responseStr}");
  }

  public void Dispose()
  {
    if (_canDisposeClient) _client.Dispose();
    _request.Dispose();
  }
}