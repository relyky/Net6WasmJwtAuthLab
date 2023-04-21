using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Reflection;
using SmallEco.Client;
using Refit;
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddBlazoredSessionStorageAsSingleton();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//## 註冊 RefitClient API。
Func<Task<string>> BearerTokenGetter = null!; // 先宣告後段再實作。
var asm = Assembly.GetAssembly(typeof(App));
foreach (var refitClientType in asm!.GetTypes().Where(c => c.Namespace == "SmallEco.Client.RefitClient" && c.IsInterface && c.Name.EndsWith("Api")))
{
  builder.Services.AddRefitClient(refitClientType, new RefitSettings
  {
    AuthorizationHeaderValueGetter = () => BearerTokenGetter()
  }).ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
}

////## 註冊 RefitClient API。 --- 手動一個一個註冊
//builder.Services
//    .AddRefitClient<ITodoApi>()
//    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

//=============================================================================
var app = builder.Build();

BearerTokenGetter = async () =>
{
  //## 自 windows session 取 Bearer token。
  var sessionStorage = app.Services.GetRequiredService<ISessionStorageService>();
  string token = await sessionStorage.GetItemAsync<string>("token");
  return token;
};

await app.RunAsync();
