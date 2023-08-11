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

//## ���U�G�Ȼs�A��
builder.Services.AddScoped<JSInterOpService>();

//## ���U RefitClient API�C --- ���������� Refit API �õ��U
var asm = Assembly.GetAssembly(typeof(App));
foreach (var refitClientType in asm!.GetTypes().Where(c => c.Namespace == "SmallEco.Client.RefitClient" && c.IsInterface && c.Name.EndsWith("Api")))
{
  builder.Services.AddRefitClient(refitClientType)
    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthHeaderHandler>(); // �N�[�J Bearer Token
}

////## ���U RefitClient API�C --- ��ʤ@�Ӥ@�ӵ��U
//builder.Services
//    .AddRefitClient<SmallEco.Client.RefitClient.ITodoApi>()
//    .ConfigureHttpClient(http => http.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
//    .AddHttpMessageHandler<AuthHeaderHandler>();

//=============================================================================
var app = builder.Build();
await app.RunAsync();
