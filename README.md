# Net6WasmJwtAuthLab in ASP.NET Core
.NET6 + WASM + Swagger + Refit => JWT Authentication

# 主要模組用途簡述
* .NET6: 執行平台
* WASM: 系統框架
* Swagger: Web API 描述文件 用 `Swashbuckle`模組實作。
* Refit: 負責 Client 與 Server 端 Web API 通訊

# 先看一下組態參數
*filepath:* `Server/appsettings.json` --- 只節取最關健的原始碼
```json
{
  "JwtSettings": {
    "Issuer": "https://rely-ky",
    "Audience": "https://rely-ky",
    "SigningKey": "showmethemoneyshowmethemoneyshowmethemoney",
    "TokenLifetimeMinutes": 600
  },
  ...略...
}
```

# 後端：設定 JWT Authentication
*filepath:* `Server/Program.cs` --- 只節取最關健的原始碼
```csharp

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

//§ Add services to the container. ============================================
[...]

builder.Services.AddAuthentication(option =>
{
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
  option.TokenValidationParameters = new TokenValidationParameters
  {
    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
    ValidAudience = builder.Configuration["JwtSettings:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SigningKey"])),
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
  };
});
builder.Services.AddAuthorization(option =>
{
  option.AddPolicy(IdentityAttr.AdminPolicyName, p =>
    p.RequireClaim(IdentityAttr.AdminClaimName, "true"));
});

//§ Configure the HTTP request pipeline. ======================================
[...]

app.UseAuthentication();
app.UseAuthorization();
```

# 後端：產生 JWT Token
*filepath:* `Server/Controllers/IdentityController.cs` --- 只節取最關健的原始碼   
參考文件：   
* [Adding JWT Authentication & Authorization in ASP.NET Core](https://www.youtube.com/watch?v=mgeuh8k3I4g&t=2s&ab_channel=NickChapsas)   
* [JwtHelpers.cs](https://github.com/doggy8088/AspNetCore6JwtAuthDemo/blob/main/JwtHelpers.cs)   
```csharp
[HttpPost("[action]")]
public IActionResult GenerateToken([FromBody] TokenGenerationRequest request)
{
  try
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SigningKey"]);

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Sub, request.Email),
      new(JwtRegisteredClaimNames.Email, request.Email),
      new("userid", request.UserId.ToString())
    };

    if (request.CustomClaims != null)
    {
      foreach (var claimPair in request.CustomClaims)
      {
        var claim = new Claim(claimPair.Key, claimPair.Value, ClaimValueTypes.String);
        claims.Add(claim);
      }
    }

    //# resource
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_config.GetValue<double>("JwtSettings:TokenLifetimeMinutes"))),
      Issuer = _config["JwtSettings:Issuer"],
      Audience = _config["JwtSettings:Audience"],
      SigningCredentials = new SigningCredentials(
              new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JwtSettings:SigningKey"])), 
              SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);

    return Ok(jwt);
  }
  catch (Exception ex)
  {
    return BadRequest(new ErrMsg(ex.Message));
  }
}
```
# 前端：設定 RefitApi 介接港口
*filepath:* `Client/RefitClient/IdentityApi.cs` 
> 後端提供多少 Web ApiController 前端就相應有多少 Refit API。 
```csharp
using Refit;
public interface IdentityApi
{
  [Post("/api/Identity/GenerateToken")]
  Task<String> GenerateTokenAsync(TokenGenerationRequest request);
}
```
# 前端：註冊 RefitApi 使可以注入(DI)
*filepath:* `Client/Program.cs`
```csharp
//## 註冊 RefitClient API。 --- 偵測全部的 Refit API 並註冊
var asm = Assembly.GetAssembly(typeof(App));
foreach (var refitClientType in asm!.GetTypes().Where(c => c.Namespace == "SmallEco.Client.RefitClient" && c.IsInterface && c.Name.EndsWith("Api")))
{
  builder.Services.AddRefitClient(refitClientType)
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>(); // 將加入 Bearer Token
}

或
//## 註冊 RefitClient API。 --- 手動一個一個註冊
builder.Services
    .AddRefitClient<SmallEco.Client.RefitClient.ITodoApi>()
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>();
```

# 前端：取得 JWT Bearer Token
*filepath:* `Client/Pages/Lab/AuthLab/_AuthLab.razor` --- 只節取最關健的原始碼   
```razor
@inject IdentityApi bizApi
@inject Blazored.SessionStorage.ISessionStorageService sessionStorage

var request = new TokenGenerationRequest
{
  UserId = new Guid("12345678-1234-1234-1234-123456789012"),
  Email = "nobody@mail.server",
  CustomClaims = new(new KeyValuePair<string, string>[]
  {
    new("admin", isAdmin ? "true" : "false")
  })
};

token = await bizApi.GenerateTokenAsync(request);
await sessionStorage.SetItemAsync("token", token);
//※ 此練習把 sessionStorage 當成 token store 使用，正式版建議存入更安全的地方或加密。
```

# 前端：Call Web API with Bearer Token
*filepath:* `Client/Pages/Lab/TodoLab/_TodoLab.razor` --- 只節取最關健的原始碼   
```razor
@inject ITodoApi bizApi

var qryArgs = new TodoQryAgs {
    Msg = f_testFail ? "測試邏輯失敗" : "今天天氣真好",
    Amt = 999
  };

dataList = await bizApi.QryDataListAsync(qryArgs);
//※ 此 WebApi 呼叫過程中 Bearer Token 由 Refit 依註冊內容中的 AuthHeaderHandler 負責加進去了。
```
>
> 若 Refit 設定與註冊做的到位就會發現可以少寫很多碼。在 Refit Api 呼叫過程中 Bearer Token 由註冊內容中的 AuthHeaderHandler 負責加進去了。
> 
*filepath:* `Client/Models/AuthHeaderHandler.cs` --- 只節取最關健的原始碼   
```csharp
using Blazored.SessionStorage;
using System.Net.Http.Headers;

class AuthHeaderHandler : DelegatingHandler
{
  readonly ISessionStorageService _sessionStorage;
  public AuthHeaderHandler(ISessionStorageService sessionStorage) //------ 將由DI注入資源
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
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); //------ 加入 Bearer token

    var resp = await base.SendAsync(request, cancellationToken);
    return resp;
  }
}
```
# 後端：設定 Authorization 以驗證有否權力使用
Authorization 的部份與標準的用法一樣，[Role-based](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-6.0)、[Policy-based](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0) 當然都可以，此範例中有：   

### `[Authorize]` 只看是否有登入
*filepath:* `Server/Controllers/TodoController.cs` --- 只節取最關健的原始碼 
```csharp
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{ ... }
```

### `[RequiresClaim(IdentityAttr.AdminClaimName,"true")]` 為系統管理者才可使用
*filepath:* `Server/Controllers/TodoController.cs` --- 只節取最關健的原始碼 
```csharp
[RequiresClaim(IdentityAttr.AdminClaimName,"true")]
[HttpPost("[action]")]
[SwaggerResponse(200, type: typeof(TodoDto))]
[SwaggerResponse(400, type: typeof(ErrMsg))]
public IActionResult AddFormData(string newTodoDesc)
{ ... }
```
(EOF)
