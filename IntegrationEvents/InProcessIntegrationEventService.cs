using MediatR;

namespace IntegrationEvents;

public class InProcessIntegrationEventService(IMediator mediator)
{
    private readonly Queue<INotification> _events = [];

    public void Add(INotification integrationEvent)
    {
        _events.Enqueue(integrationEvent);
    }

    public async Task DispatchIntegrationEventsAsync()
    {
        while (_events.Count > 0)
        {
            var integrationEvent = _events.Dequeue();
            await mediator.Publish(integrationEvent);
        }
    }
}
