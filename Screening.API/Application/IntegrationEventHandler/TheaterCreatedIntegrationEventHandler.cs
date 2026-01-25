using MediatR;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Theater.IntegrationEvent;
using TheaterEntity = Screening.Domain.Aggregate.TheaterAggregate.Theater;

namespace Screening.API.Application.IntegrationEventHandler;

public class TheaterCreatedIntegrationEventHandler(ITheaterRepository theaterRepository)
    : INotificationHandler<TheaterCreatedIntegrationEvent>
{
    public async Task Handle(TheaterCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var seatCodes = @event.SeatCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new SeatCode(x))
            .Distinct()
            .ToArray();

        var theater = new TheaterEntity(@event.TheaterId, seatCodes);
        theaterRepository.Add(theater);

        await theaterRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}