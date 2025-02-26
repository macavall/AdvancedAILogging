using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

public class LoggingResultsFunction
{
    private readonly ILoggingExplorerResultService _resultService;

    public LoggingResultsFunction(ILoggingExplorerResultService resultService)
    {
        _resultService = resultService;
    }

    [Function("GetLoggingResults")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        var providers = _resultService.GetProviders();
        var categories = _resultService.GetCategories();

        var result = new
        {
            Providers = providers,
            Categories = categories
        };

        return new OkObjectResult(result);
    }
}