using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using System.Threading.Tasks;

public class SbClient : ISbClient
{
    private readonly ServiceBusClient _sbClient;
    public SbClient(IAzureClientFactory<ServiceBusClient> clientFactory)
    {
        _sbClient = clientFactory.CreateClient("sbClient");
    }
    public async Task SendMessageAsync()
    {
        // Create a sender for a specific queue or topic (replace "destination-queue" with your target)
        await using var sender = _sbClient.CreateSender("queue1");

        // Create and send a message
        var serviceMessage = new ServiceBusMessage("Hello from the ServiceBusClient!");
        await sender.SendMessageAsync(serviceMessage);
    }
}