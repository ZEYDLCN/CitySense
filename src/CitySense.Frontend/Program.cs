using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CitySense.Frontend;
using CitySense.Frontend.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7179") }); 

builder.Services.AddScoped<SensorDataService>();
var apiBaseAddress = "https://localhost:7179";
builder.Services.AddSingleton(sp => new SignalRService($"{apiBaseAddress}/sensorDataHub"));


await builder.Build().RunAsync();