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


# 前端：Call Web API with Bearer Token


# 後端：設定 Authorization 以驗證有否權力使用



