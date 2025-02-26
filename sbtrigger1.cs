//using System;
//using System.Threading.Tasks;
//using Azure.Messaging.ServiceBus;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Azure;
//using Microsoft.Extensions.Logging;

//namespace serbus5656fa
//{
//    public class sbtrigger1
//    {
//        private readonly ILogger<sbtrigger1> _logger;
//        private readonly ISbClient _sbClient;

//        public sbtrigger1(ILogger<sbtrigger1> logger, ISbClient sbClient)
//        {
//            _logger = logger;
//            _sbClient = sbClient;
//        }

//        [Function(nameof(sbtrigger1))]
//        public async Task Run(
//            [ServiceBusTrigger("queue1", Connection = "sbconnstring", IsBatched = true)]
//            ServiceBusReceivedMessage[] message,
//            ServiceBusMessageActions messageActions)
//        {
//            foreach (var msg in message)
//            {
//                _logger.LogInformation("Message ID: {id}", msg.MessageId);
//                _logger.LogInformation("Message Body: {body}", msg.Body);
//                _logger.LogInformation("Message Content-Type: {contentType}", msg.ContentType);

//                // Create a sender for a specific queue or topic (replace "destination-queue" with your target)

//                _ = Task.Factory.StartNew(async () =>
//                {
//                    await _sbClient.SendMessageAsync();
//                });

//                // Complete the message
//                await messageActions.CompleteMessageAsync(msg);
//            }
//        }
//    }
//}
