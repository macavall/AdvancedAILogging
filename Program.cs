using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            .AddSingleton<ILoggingExplorerResultService, LoggingExplorerResultService>()
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
            // Step 1: Inspect logging providers during configuration
            builder.Services
            .AddLogging(logging =>
            {
                logging.AddConsole(); // Ensure console output for local debugging
                Console.WriteLine("Registered Logger Providers during Configuration:");

                logging.AddApplicationInsights(
                    configure => configure.IncludeScopes = true,
                    options => { }
                    );

                foreach (var provider in logging.Services.Where(s => s.ServiceType == typeof(ILoggerProvider)))
                {
                    Console.WriteLine($"- {provider.ImplementationType?.FullName ?? "Unknown Provider"}");
                }
            })
            // Step 2: Add a hosted service to explore logging after build
            .AddHostedService<LoggerExplorerService>();

        var app = builder.Build();
        await app.RunAsync();
    }
}

// Custom service to explore logging categories
//public class LoggerExplorerService : IHostedService
//{
//    private readonly ILogger<LoggerExplorerService> _logger;
//    private readonly ILoggerFactory _loggerFactory; // Inject ILoggerFactory directly

//    public LoggerExplorerService(ILogger<LoggerExplorerService> logger, ILoggerFactory loggerFactory)
//    {
//        _logger = logger;
//        _loggerFactory = loggerFactory;
//    }

//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        // Step 3: List providers using the injected ILoggerFactory
//        Console.WriteLine("\nActive ILogger Providers:");
//        var providersField = _loggerFactory.GetType().GetField("_loggerProviders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        if (providersField?.GetValue(_loggerFactory) is IEnumerable<object> providers)
//        {
//            foreach (var provider in providers)
//            {
//                Console.WriteLine($"- {provider.GetType().FullName}");
//            }
//        }

//        // Step 4: Test common logging categories
//        Console.WriteLine("\nTesting Common Logging Categories:");
//        TestCategory("Host");
//        TestCategory("Host.ScaleMonitor");
//        TestCategory("Host.Aggregator");
//        TestCategory("Host.Triggers.ServiceBus");
//        TestCategory("Function");
//        TestCategory("Microsoft.Azure.WebJobs.ServiceBus");
//        TestCategory("Microsoft.Azure.Functions.Worker");
//        TestCategory("Host.Controllers.Scale");
//        TestCategory("Microsoft.ApplicationInsights");
//        TestCategory("Azure.Messaging.ServiceBus");

//        return Task.CompletedTask;
//    }

//    private void TestCategory(string category)
//    {
//        var testLogger = _loggerFactory.CreateLogger(category); // Use the injected ILoggerFactory
//        testLogger.LogInformation($"Testing logging category: {category}");
//        Console.WriteLine($"- {category}");
//    }

//    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
//}

// Placeholder for ISbClient and SbClient (assuming these exist in your codebase)
//public interface ISbClient { }
//public class SbClient : ISbClient { }

public interface ILoggingExplorerResultService
{
    void AddProvider(string providerName);
    void AddCategory(string categoryName);
    List<string> GetProviders();
    List<string> GetCategories();
}

public class LoggingExplorerResultService : ILoggingExplorerResultService
{
    private readonly List<string> _providers = new List<string>();
    private readonly List<string> _categories = new List<string>();

    public void AddProvider(string providerName) => _providers.Add(providerName);
    public void AddCategory(string categoryName) => _categories.Add(categoryName);
    public List<string> GetProviders() => _providers;
    public List<string> GetCategories() => _categories;
}

public class LoggerExplorerService : IHostedService
{
    private readonly ILogger<LoggerExplorerService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILoggingExplorerResultService _resultService;

    public LoggerExplorerService(
        ILogger<LoggerExplorerService> logger,
        ILoggerFactory loggerFactory,
        ILoggingExplorerResultService resultService)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _resultService = resultService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // List providers
        var providersField = _loggerFactory.GetType().GetField("_loggerProviders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (providersField?.GetValue(_loggerFactory) is IEnumerable<object> providers)
        {
            foreach (var provider in providers)
            {
                var providerName = provider.GetType().FullName;
                _resultService.AddProvider(providerName);
            }
        }

        // Test common logging categories
        TestCategory("Host");
        TestCategory("Host.ScaleMonitor");
        TestCategory("Host.Aggregator");
        TestCategory("Host.Triggers.ServiceBus");
        TestCategory("Function");
        TestCategory("Microsoft.Azure.WebJobs.ServiceBus");
        TestCategory("Microsoft.Azure.Functions.Worker");
        TestCategory("Host.Controllers.Scale");
        TestCategory("Microsoft.ApplicationInsights");
        TestCategory("Azure.Messaging.ServiceBus");

        return Task.CompletedTask;
    }

    private void TestCategory(string category)
    {
        var testLogger = _loggerFactory.CreateLogger(category);
        testLogger.LogInformation($"Testing logging category: {category}");
        _resultService.AddCategory(category);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}