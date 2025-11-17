using GameTribunal.Application.Contracts;
using GameTribunal.Application.Services;
using GameTribunal.Infrastructure.Persistence;
using GameTribunal.Web.Components;
using GameTribunal.Web.Hubs;
using GameTribunal.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR for real-time communication (RF-011, RF-012)
builder.Services.AddSignalR();

builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
builder.Services.AddSingleton<IRoomCodeGenerator, RandomRoomCodeGenerator>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<SignalRRoomService>(); // SignalR-aware wrapper (RF-011)
builder.Services.AddSingleton<QrCodeService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedHost |
        ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();

app.Use((HttpContext context, RequestDelegate next) =>
{
    var scope = context.RequestServices.CreateScope();
    var configuration = scope.ServiceProvider.GetService<IConfiguration>();
    var request = context.Request;
    configuration["BaseUrl"] = $"{request.Scheme}://{request.Host}";
    return next(context);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hub endpoint (RF-011, RF-012)
app.MapHub<GameHub>("/gamehub");

app.Run();
