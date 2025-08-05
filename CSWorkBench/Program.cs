using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorStrap;
using DynObjectStore;
using CSWorkBench;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazorStrap();

// global Registry
string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=CSWorkBenchDB";
Registry registry = new Registry(new PgDBConnection(connectionString));
builder.Services.AddSingleton(registry);

await builder.Build().RunAsync();
