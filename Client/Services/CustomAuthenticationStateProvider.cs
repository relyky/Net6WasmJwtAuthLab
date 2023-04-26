using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text.Json;

namespace SmallEco.Client.Services;

/// <summary>
/// 加值預設的 AuthenticationStateProvider。
/// </summary>
/// <remarks>
/// Blazor Tutorial : Authentication | Custom AuthenticationStateProvider - EP12
/// 請參考： https://www.youtube.com/watch?v=BmAnSNfFGsc&list=PL4WEkbdagHIR0RBe_P4bai64UDqZEbQap&index=12&ab_channel=CuriousDrive
/// AuthenticationStateProvider vs AuthenticationState
/// 請參考： https://www.eugenechiang.com/2020/12/26/authenticationstateprovider-vs-authenticationstate/
/// </remarks>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
  readonly ISessionStorageService _sessionStorage;

  // resource
  readonly AuthenticationState anonymousUser;

  public CustomAuthenticationStateProvider(ISessionStorageService sessionStorage)
  {
    _sessionStorage = sessionStorage;
    //※ 此練習把 sessionStorage 當成 token store 使用，正式版建議存入更安全的地方或加密。

    anonymousUser = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
  }

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    //## 自 token 存放庫取得
    string token = await _sessionStorage.GetItemAsync<string>("token");
    if (!String.IsNullOrWhiteSpace(token))
    {
      var userClaims = ParseClaimsFromJwt(token);

      //# 取登入資訊完成
      var userAuthState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(userClaims, "JWT", JwtRegisteredClaimNames.GivenName, null)));
      NotifyAuthenticationStateChanged(Task.FromResult(userAuthState));
      return userAuthState;
    }

    //## 預設未登入
    NotifyAuthenticationStateChanged(Task.FromResult(anonymousUser));
    return anonymousUser;
  }

  static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
  {
    var payload = jwt.Split('.')[1];
    var jsonBytes = ParseBase64WithoutPadding(payload);
    var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
    return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
  }

  static byte[] ParseBase64WithoutPadding(string base64)
  {
    switch (base64.Length % 4)
    {
      case 2: base64 += "=="; break;
      case 3: base64 += "="; break;
    }
    return Convert.FromBase64String(base64);
  }
}
