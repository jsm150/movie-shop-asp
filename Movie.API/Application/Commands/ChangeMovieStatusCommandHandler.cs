using MediatR;
using Movie.API.Infrastructure;
using Movie.Domain.Exceptions;
using Movie.IntegrationEvent;
using MovieStatus = Movie.Domain.Aggregate.MovieStatus;

namespace Movie.API.Application.Commands;

public class ChangeMovieStatusCommandHandler(
    IMovieContext context, 
    IMediator mediator
) 
    : IRequestHandler<ChangeMovieStatusCommand, bool>
{
    public async Task<bool> Handle(ChangeMovieStatusCommand request, CancellationToken cancellationToken)
    {
        var movie = await context.Movies.FindAsync(request.MovieId)
            ?? throw new MovieDomainException("대상 영화를 찾을 수 없습니다.");

        if (movie.MovieStatus == request.Status)
        {
            return true;
        }

        switch (request.Status)
        {
            case MovieStatus.COMMING_SOON:
                movie.MoveToCommingSoon();
                break;

            case MovieStatus.NOW_SHOWING:
                movie.StartShowing();
                break;

            case MovieStatus.ENDED:
                movie.EndShowing();
                break;

            case MovieStatus.PREPARING:
            default:
                throw new MovieDomainException("지원하지 않는 상태 변경입니다.");
        }
        
        await context.SaveEntitiesAsync(cancellationToken);

        var integrationEvent = new MovieStatusChangedIntegrationEvent(
            movie.MovieId,
            movie.MovieStatus switch
            {
                MovieStatus.PREPARING => IntegrationEvent.MovieStatus.PREPARING,
                MovieStatus.COMMING_SOON => IntegrationEvent.MovieStatus.COMMING_SOON,
                MovieStatus.NOW_SHOWING => IntegrationEvent.MovieStatus.NOW_SHOWING,
                MovieStatus.ENDED => IntegrationEvent.MovieStatus.ENDED,
                _ => throw new NotImplementedException(),
            }
        );

        await mediator.Publish(integrationEvent, cancellationToken);

        return true;
    }
}