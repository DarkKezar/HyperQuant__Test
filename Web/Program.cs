using System.Net.WebSockets;
using Core.Connectors.TestConnector;
using Service.Services;

var builder = WebApplication.CreateBuilder(args);

#region DI
// TODO
// 1. Program.cs -> Extensions
// 2. Lifetime
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<ClientWebSocket>();
builder.Services.AddSingleton<CancellationTokenSource>();
builder.Services.AddSingleton<ITestRestConnector, TestRestConnector>();
builder.Services.AddSingleton<ITestWSConnector, TestWsConnector>();
builder.Services.AddSingleton<IConnectorService, ConnectorService>(); 
#endregion

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();