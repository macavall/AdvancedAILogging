using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace serbus5656fa
{
    public class sbtrigger1
    {
        private readonly ILogger<sbtrigger1> _logger;

        public sbtrigger1(ILogger<sbtrigger1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(sbtrigger1))]
        public async Task Run(
            [ServiceBusTrigger("queue1", Connection = "sbconnstring", IsBatched = true)]
            ServiceBusReceivedMessage[] message,
            ServiceBusMessageActions messageActions)
        {
            foreach (var msg in message)
            {
                _logger.LogInformation("Message ID: {id}", msg.MessageId);
                _logger.LogInformation("Message Body: {body}", msg.Body);
                _logger.LogInformation("Message Content-Type: {contentType}", msg.ContentType);

                // Complete the message
                await messageActions.CompleteMessageAsync(msg);
            }
        }
    }
}
