using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Reflection;
using SmallEco.Client;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//## ���U RefitClient API�C
var asm = Assembly.GetAssembly(typeof(App));
foreach (var refitClientType in asm.GetTypes().Where(c => c.Namespace == "SmallEco.Client.RefitClient" && c.IsInterface && c.Name.EndsWith("Api")))
{
  builder.Services.AddRefitClient(refitClientType)
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
}

////## ���U RefitClient API�C --- ��ʤ@�Ӥ@�ӵ��U
//builder.Services
//    .AddRefitClient<ITodoApi>()
//    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync(); 
