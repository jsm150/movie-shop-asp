using MediatR;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Domain.Exceptions;

namespace Screening.API.Application.Commands;

public class CreateScreenCommandHandler
(
    IScreeningMovieRepository movieRepository,
    ITheaterRepository theaterRepository,
    IScreenRepository screenRepository
)
    : IRequestHandler<CreateScreenCommand, long>
{
    public async Task<long> Handle(CreateScreenCommand request, CancellationToken cancellationToken)
    {
        var movie = await movieRepository.FindAsync(request.MovieId)
            ?? throw new ScreeningDomainException("상영할 영화를 찾을 수 없습니다.");

        var theater = await theaterRepository.FindAsync(request.TheaterId)
            ?? throw new ScreeningDomainException("상영관을 찾을 수 없습니다.");

        var screen = await Screen.CreateAsync(
            movie,
            theater.TheaterId,
            screenRepository,
            request.StartTime,
            request.EndTime,
            request.SalesStartAt,
            request.SalesEndAt);

        screenRepository.Add(screen);
        await screenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return screen.ScreenId;
    }
}