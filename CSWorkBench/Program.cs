using CSWorkBench;
using CSWorkBench.Components.Layout;
using DynObjectStore;
using Microsoft.JSInterop;

AssemblyResolver.AddAssemblyResolvers();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Properties"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// connection setup
IDBConnection conn = new SQLiteDBConnection(@"..\data\test.db"); //TODO this is temp
var registry = new Registry(conn);
conn.Open();
builder.Services.AddSingleton(registry);

// reflection setup
var reflectionService = new ReflectionService();
builder.Services.AddSingleton(reflectionService);

// user setup
UserService userService = new UserService();
builder.Services.AddSingleton(userService);
builder.Services.AddScoped(sp => new SessionSettings(userService, sp.GetRequiredService<IJSRuntime>()));

// build
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

// cleanup
app.Lifetime.ApplicationStopping.Register(() =>
{
    conn.Dispose();
    userService.Save();
    });

app.Run();
