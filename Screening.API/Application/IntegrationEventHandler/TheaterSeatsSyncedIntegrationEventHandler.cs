using MediatR;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Theater.IntegrationEvent;
using TheaterEntity = Screening.Domain.Aggregate.TheaterAggregate.Theater;

namespace Screening.API.Application.IntegrationEventHandler;

public class TheaterSeatsSyncedIntegrationEventHandler
(
    IScreenRepository screenRepository,
    ITheaterRepository theaterRepository
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

        var theater = await theaterRepository.FindAsync(@event.TheaterId);
        if (theater is null)
        {
            theater = new TheaterEntity(@event.TheaterId, seatCodes);
            theaterRepository.Add(theater);
        }
        else
        {
            var screen = await screenRepository.FindByTheaterId(theater.TheaterId);
            theater.ReplaceSeats(seatCodes, screen);
        }

        await theaterRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}