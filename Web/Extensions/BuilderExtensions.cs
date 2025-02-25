using System.Net.WebSockets;
using Core.Connectors.TestConnector;
using Service.Services;

namespace Web.Extensions;

public static class BuilderExtensions
{
    public static void DependencyInjection(this WebApplicationBuilder builder)
    {
        builder.CoreInjection();
        builder.ServicesInjection();
    }

    private static void CoreInjection(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ITestRestConnector, TestRestConnector>();
        builder.Services.AddSingleton<ITestWSConnector, TestWsConnector>();
    }

    private static void ServicesInjection(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnectorService, ConnectorService>(); 
    }
}