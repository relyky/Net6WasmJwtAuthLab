# Net6WasmJwtAuthLab in ASP.NET Core
.NET6 + WASM + Swagger + Refit => JWT Authentication

# 主要模組用途簡述
* .NET6: 執行平台
* WASM: 系統框架
* Swagger: Web API 描述文件
* Refit: 負責 Client 與 Server 端 Web API 通訊

# 先看一下組態參數
```csharp
stirng a = "今天天氣真好。";
```

# 設定 JWT Authenticatio
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

# 產生 JWT Token


# Web Api with Bearer Token


# Authorization 

