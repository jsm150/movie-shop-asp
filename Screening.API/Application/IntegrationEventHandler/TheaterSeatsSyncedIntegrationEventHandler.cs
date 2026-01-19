using IntegrationEvents.Events;
using MediatR;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;

namespace Screening.API.Application.IntegrationEventHandler;

public class TheaterSeatsSyncedIntegrationEventHandler(
    ITheaterRepository theaterRepository,
    IScreenRepository screenRepository
)
    : INotificationHandler<TheaterSeatsSyncedIntegrationEvent>
{
    public async Task Handle(TheaterSeatsSyncedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var seatCodes = @event.SeatCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new SeatCode(x))
            .Distinct()
            .ToArray();

        var theater = await theaterRepository.GetAsync(@event.TheaterId, cancellationToken);
        if (theater is null)
        {
            theater = new Theater(@event.TheaterId, seatCodes);
            theaterRepository.Add(theater);
        }
        else
        {
            var screen = await screenRepository.FindByTheaterIdAsync(theater.TheaterId, cancellationToken);
            theater.ReplaceSeats(seatCodes, screen);
        }

        await theaterRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}