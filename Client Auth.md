# Client Side Auth. 紀錄
在 WASM 的授權管理，除了後端 Web API 必需管制。前端的 @page 用戶是否有權開啟使用也是管制的項目之一。  
在實作上 Blazor WASM App 的 Client 授權管制的指令與 Blazor Server App 是一樣的，都是以 AuthenticationStateProvider 為中心。

參考文章：[Blazor WASM App 驗證與授權](https://rely-ky.gitbook.io/gitbook/blazor-wasm-app-yan-zheng-yu-shou-quan)

# 登入認證概述
1. 為客製化方案，全程使用 Bearer token 為認證基礎。
2. 後端 Api 授權管制與 Web API 是一致的。
3. 前端 @page 的管制與 Blazor Server App 一樣也是以 AuthenticationStateProvider 為中心。所以也是用 Task<AuthenticationState> 來取認證狀態。
4. 授權認證模組也是用：`Microsoft.AspNetCore.Components.Authorization`。
  
> ※注意：為練習用，只為確認有登入認證與授權效果，正式版應用應再強化或重構使更成熟可靠。

# 前端：CustomAuthenticationStateProvider
*filepath:* `Client/Services/CustomAuthenticationStateProvider.cs`  --- 只節取最關健的原始碼  
```csharp
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
  [...]
  
  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    //## 自 token 存放庫取得
    string token = await _sessionStorage.GetItemAsync<string>("token");
    if (!String.IsNullOrWhiteSpace(token))
    {
      var userClaims = ParseClaimsFromJwt(token);

      //# 取登入資訊完成
      var userAuthState = new AuthenticationState(new ClaimsPrincipal(
	      new ClaimsIdentity(userClaims, "JWT", JwtRegisteredClaimNames.GivenName, null)));
		  
	  //# 成功取得登入資訊後，通知登入狀態已改變。
      NotifyAuthenticationStateChanged(Task.FromResult(userAuthState));
      return userAuthState;
    }

    //## 預設未登入(或已登出)，通知登入狀態已改變。
    NotifyAuthenticationStateChanged(Task.FromResult(anonymousUser));
    return anonymousUser;
  }

  [...]
}
```

# 前端：Program.cs 註冊 AuthenticationStateProvider
*filepath:* `Client/Program.cs`   --- 只節取最關健的原始碼   
```csharp

//## for Authentication & Authorization
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

```

# 前端：登入與登出介面
登入的後端動作就是產生 JWT Bearer token，這與前端相同不再說明。   
為簡化框架用 window.sessionStorage 放 token 正式版建議用更安全的方式。
	
## 登入關鍵碼
*filepath:* `Client/Pages/Lab/AuthLab/_AuthLab.razor`   --- 只節取最關健的原始碼   
```csharp
@inject Blazored.SessionStorage.ISessionStorageService sessionStorage
@inject AuthenticationStateProvider authStateProvider

...略...
	
    token = await bizApi.GenerateTokenAsync(request); // 登入的程序就是為了取到 auth token。
    await sessionStorage.SetItemAsync("token", token); 
    //※ 此練習把 sessionStorage 當成 token store 使用，正式版建議存入更安全的地方或加密。

    // 將會刷新登入狀態
    await authStateProvider.GetAuthenticationStateAsync();

```	
	
## 登出關鍵碼
*filepath:* `Client/Shared/MainLayout.razor`   --- 只節取最關健的原始碼   
```csharp
@inject Blazored.SessionStorage.ISessionStorageService sessionStorage
@inject AuthenticationStateProvider authStateProvider

...略...
	
  async Task HandleLogout()
  {
    // 清除 auth token
    await sessionStorage.RemoveItemAsync("token"); 
    // 刷新登入狀態
    await authStateProvider.GetAuthenticationStateAsync();
  }
```	
	
# 前端：在 @page 取得登入狀態
> 方法一樣用 `Task<AuthenticationState>` 或 `<AuthorizeView />` 元，這與 Blazor Server App 一致。   
	
*filepath:* `Client/Pages/Lab/AuthStateLab/_AuthStateLab.razor`   --- 只節取最關健的原始碼   
```csharp
@page "/authstate"
@attribute [Authorize]

<PageTitle>Authetication State</PageTitle>

<MudContainer>
  <MudText Typo=Typo.h3>Authetication State</MudText>

  <AuthorizeView Context="auth">
    <p>
      Name: @auth.User.Identity?.Name <br />
      IsAuthenticated: @auth.User.Identity?.IsAuthenticated
    </p>
  </AuthorizeView>

  @if (userIdentity != null)
  {
     @* ...render userIdentity... *@    
  }

</MudContainer>

@code {
  [CascadingParameter] Task<AuthenticationState> AuthState { get; set; }

  //## State
  ClaimsIdentity? userIdentity = null;

  protected override async Task OnParametersSetAsync()
  {
    await base.OnParametersSetAsync();
    var authState = await AuthState;
    userIdentity = authState?.User?.Identity as ClaimsIdentity;
  }

}
```	
