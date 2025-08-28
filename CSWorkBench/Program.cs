using BlazorStrap;
using CSWorkBench.Components.Layout;
using DynObjectStore;

AssemblyResolver.AddAssemblyResolvers();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Properties"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorStrap();

// connection setup
IDBConnection conn = new SQLiteDBConnection(@"..\data\test.db"); //TODO this is temp
var registry = new Registry(conn);
builder.Services.AddSingleton(registry);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() => conn.Open());

app.Lifetime.ApplicationStopping.Register(() => conn.Dispose());

app.Run();
