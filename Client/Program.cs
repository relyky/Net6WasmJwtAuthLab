using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Refit;
using SmallEco.Client;
using SmallEco.Client.Services;
using SmallEco.Models;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//## for Authentication & Authorization
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices();
builder.Services.AddBlazoredSessionStorageAsSingleton();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<AuthHeaderHandler>();

//## 註冊：客製服務
builder.Services.AddScoped<JSInterOpService>();

//## 註冊 RefitClient API。 --- 偵測全部的 Refit API 並註冊
var asm = Assembly.GetAssembly(typeof(App));
foreach (var refitClientType in asm!.GetTypes().Where(c => c.Namespace == "SmallEco.Client.RefitClient" && c.IsInterface && c.Name.EndsWith("Api")))
{
  builder.Services.AddRefitClient(refitClientType)
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>(); // 將加入 Bearer Token
}

////## 註冊 RefitClient API。 --- 手動一個一個註冊
//builder.Services
//    .AddRefitClient<SmallEco.Client.RefitClient.ITodoApi>()
//    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
//    .AddHttpMessageHandler<AuthHeaderHandler>();

//=============================================================================
var app = builder.Build();
await app.RunAsync();
