using System;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace serbus5656fa
{
    public class ehtrigger1
    {
        private readonly ILogger<ehtrigger1> _logger;

        public ehtrigger1(ILogger<ehtrigger1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ehtrigger1))]
        public void Run([EventHubTrigger("eventhub1", Connection = "ehconnstring")] EventData[] events)
        {
            foreach (EventData @event in events)
            {
                _logger.LogInformation("Event Body: {body}", @event.Body);
                _logger.LogInformation("Event Content-Type: {contentType}", @event.ContentType);
            }
        }
    }
}
