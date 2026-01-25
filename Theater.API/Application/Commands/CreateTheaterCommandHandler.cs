using MediatR;
using Theater.Domain.Aggregate;
using Theater.IntegrationEvent;
using TheaterEntity = Theater.Domain.Aggregate.Theater;

namespace Theater.API.Application.Commands;

public class CreateTheaterCommandHandler(ITheaterRepository theaterRepository, IMediator mediator)
    : IRequestHandler<CreateTheaterCommand, long>
{
    public async Task<long> Handle(CreateTheaterCommand request, CancellationToken cancellationToken)
    {
        // Theater 엔티티 생성
        var theater = await TheaterEntity.CreateAsync(
            theaterRepository: theaterRepository,
            name: request.Name,
            floor: request.Floor,
            type: request.Type, 
            seats: request.Seats,
            rowCount: request.RowCount,
            columnCount: request.ColumnCount
        );

        theaterRepository.Add(theater);
        await theaterRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        // 통합 이벤트 발행
        var integrationEvent = new TheaterCreatedIntegrationEvent()
        {
            TheaterId = theater.TheaterId,
            SeatCodes = theater.Seats.Select(seat => seat.SeatCode).ToList()
        };

        await mediator.Publish(integrationEvent, cancellationToken);

        return theater.TheaterId;
    }
}