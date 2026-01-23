using MediatR;
using Microsoft.EntityFrameworkCore;
using Screening.API.Infrastructure;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Exceptions;

namespace Screening.API.Application.Commands;

public class CreateScreenCommandHandler(IScreeningContext context)
    : IRequestHandler<CreateScreenCommand, long>
{
    public async Task<long> Handle(CreateScreenCommand request, CancellationToken cancellationToken)
    {
        var movie = await context.ScreeningMovies.FindAsync(request.MovieId)
            ?? throw new ScreeningDomainException("상영할 영화를 찾을 수 없습니다.");

        var theater = await context.Theaters.FindAsync(request.TheaterId)
            ?? throw new ScreeningDomainException("상영관을 찾을 수 없습니다.");

        var hasConflict = await context.Screens.AnyAsync(
            s => s.TheaterId == request.TheaterId &&
                 s.StartTime < request.EndTime &&
                 request.StartTime < s.EndTime,
            cancellationToken);

        if (hasConflict)
            throw new ScreeningDomainException("요청 시간에 해당 상영관의 상영이 이미 예약되어 있습니다.");

        var screen = new Screen(
            movie,
            request.StartTime,
            request.EndTime,
            request.SalesStartAt,
            request.SalesEndAt)
        {
            TheaterId = theater.TheaterId
        };

        context.Screens.Add(screen);
        await context.SaveEntitiesAsync(cancellationToken);

        return screen.ScreenId;
    }
}