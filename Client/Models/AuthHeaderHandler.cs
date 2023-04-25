using Blazored.SessionStorage;
using System.Net.Http.Headers;

namespace SmallEco.Models;

/// <summary>
/// Custom delegating handler for adding Auth headers to outbound requests
/// </summary>
class AuthHeaderHandler : DelegatingHandler
{
  readonly ISessionStorageService _sessionStorage;

  public AuthHeaderHandler(ISessionStorageService sessionStorage)
  {
    _sessionStorage = sessionStorage;
    //※ 此練習把 sessionStorage 當成 token store 使用，正式版建議存入更安全的地方或加密。
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    //## 自 token 存放庫取得
    string token = await _sessionStorage.GetItemAsync<string>("token");

    //※ potentially refresh token here if it has expired etc.

    if (!String.IsNullOrWhiteSpace(token))
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var resp = await base.SendAsync(request, cancellationToken);
    return resp;
  }
}