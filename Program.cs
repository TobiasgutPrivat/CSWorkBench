using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CSWorkBench;
using BlazorStrap;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazorStrap();

// global Registry
// string connectionString = "Host=localhost;Port=5432;Username=root;Password=root;Database=CSWorkBenchDB";
// Registry registry = new Registry(new DBConnection(connectionString));
// builder.Services.AddSingleton(registry);

await builder.Build().RunAsync();
