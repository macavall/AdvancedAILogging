using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();

        // Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
        builder.Services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddSingleton<ISbClient, SbClient>()
            .Configure<TelemetryConfiguration>((config) =>
            {
                // Find the AdaptiveSamplingTelemetryProcessor and disable sampling by setting percentage to 100%
                var adaptiveSamplingProcessor = config.DefaultTelemetrySink.TelemetryProcessors
                    .OfType<AdaptiveSamplingTelemetryProcessor>()
                    .FirstOrDefault();

                if (adaptiveSamplingProcessor != null)
                {
                    adaptiveSamplingProcessor.MinSamplingPercentage = 100;
                }
            })
            .AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddServiceBusClient(Environment.GetEnvironmentVariable("sbconnstring"))
                .WithName("sbClient");
            });

        await builder.Build().RunAsync();
    }
}