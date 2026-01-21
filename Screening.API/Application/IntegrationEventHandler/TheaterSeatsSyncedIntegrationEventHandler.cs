using MediatR;
using Microsoft.EntityFrameworkCore;
using Screening.API.Infrastructure;
using Screening.Domain.Aggregate.TheaterAggregate;
using Theater.IntegrationEvent;
using TheaterEntity = Screening.Domain.Aggregate.TheaterAggregate.Theater;

namespace Screening.API.Application.IntegrationEventHandler;

public class TheaterSeatsSyncedIntegrationEventHandler(IScreeningContext context)
    : INotificationHandler<TheaterSeatsSyncedIntegrationEvent>
{
    public async Task Handle(TheaterSeatsSyncedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var seatCodes = @event.SeatCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new SeatCode(x))
            .Distinct()
            .ToArray();

        var theater = await context.Theaters.FindAsync(@event.TheaterId);
        if (theater is null)
        {
            theater = new TheaterEntity(@event.TheaterId, seatCodes);
            context.Theaters.Add(theater);
        }
        else
        {
            var screen = await context.Screens.FirstOrDefaultAsync(s => s.TheaterId == theater.TheaterId);
            theater.ReplaceSeats(seatCodes, screen);
        }

        await context.SaveEntitiesAsync(cancellationToken);
    }
}