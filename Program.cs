using BalizasV16.Components;
using BalizasV16.Models;
using BalizasV16.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiKeys"));
builder.Services.AddHttpClient("DGT", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/xml, text/xml, */*");
    client.DefaultRequestHeaders.Add("User-Agent", "BalizasV16-BlazorApp/1.0");
});
builder.Services.AddHttpClient("Overpass", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("User-Agent", "TacoMap-BlazorApp/1.0");
});
builder.Services.AddHttpClient("Yelp", client =>
{
    client.BaseAddress = new Uri("https://api.yelp.com/v3/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient("GooglePlaces", client =>
{
    client.BaseAddress = new Uri("https://places.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient("Foursquare", client =>
{
    client.BaseAddress = new Uri("https://api.foursquare.com/v3/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient("USGS", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "BalizasV16-BlazorApp/1.0");
});
builder.Services.AddHttpClient("SSN", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "BalizasV16-BlazorApp/1.0");
});
builder.Services.AddHttpClient("CENAPRED", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "BalizasV16-BlazorApp/1.0");
});
builder.Services.AddHostedService<BalizaService>();
builder.Services.AddHostedService<EarthquakeService>();
builder.Services.AddHostedService<VolcanoService>();
builder.Services.AddSingleton<TacoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHub<BalizaHub>("/balizahub");
app.MapHub<TacoHub>("/tacohub");
app.MapHub<EarthquakeHub>("/earthquakehub");
app.MapHub<VolcanoHub>("/volcanohub");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
