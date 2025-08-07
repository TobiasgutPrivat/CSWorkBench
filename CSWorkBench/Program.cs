using BlazorStrap;
using CSWorkBench.Components;
using DynObjectStore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorStrap();

// // PostgreSQL connection setup
// var connBuilder = new NpgsqlConnectionStringBuilder
// {
//     Host = "localhost",
//     Port = 5432,
//     Username = "postgres",
//     Password = "postgres",
//     Database = "CSWorkBenchDB",
//     SslMode = SslMode.Disable
// };

// IDBConnection conn = new PgDBConnection(connBuilder.ConnectionString);
// var registry = new Registry(conn);
// builder.Services.AddSingleton(registry);

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

app.Run();
