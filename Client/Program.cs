using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Reflection;
using SmallEco.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//## 註冊 SwagClient API。
var asm = Assembly.GetAssembly(typeof(App));
foreach (var swagClientType in asm.GetTypes().Where(c => c.Namespace == "SwagClient" && c.Name.EndsWith("Api")))
{
  builder.Services.AddScoped(swagClientType, provider =>
  {
    var http = provider.GetRequiredService<HttpClient>();
    var swagClient = Activator.CreateInstance(swagClientType, builder.HostEnvironment.BaseAddress, http);
    return swagClient;
  });
}

//## 註冊 SwagClient API。 --- 手動一個一個註冊
//builder.Services.AddScoped<SwagClient.WeatherForecastApi>(provider =>
//{
//  var http = provider.GetRequiredService<HttpClient>();
//  var env = provider.GetRequiredService<IWebAssemblyHostEnvironment>();
//  return new SwagClient.WeatherForecastApi(env.BaseAddress, http);
//});

await builder.Build().RunAsync();
