using IntegrationEvents;
using IntegrationEvents.Events;
using MediatR;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using MovieStatus = Movie.Domain.Aggregate.MovieStatus;

namespace Movie.API.Application.Commands;

public class ChangeMovieStatusCommandHandler(IMovieRepository movieRepository, InProcessIntegrationEventService integrationEventService) : IRequestHandler<ChangeMovieStatusCommand, bool>
{
    public async Task<bool> Handle(ChangeMovieStatusCommand request, CancellationToken cancellationToken)
    {
        var movie = await movieRepository.GetAsync(request.MovieId)
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

        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var integrationEvent = new MovieStatusChangedIntegrationEvent(
            movie.MovieId,
            movie.MovieStatus switch
            {
                MovieStatus.PREPARING => IntegrationEvents.Events.MovieStatus.PREPARING,
                MovieStatus.COMMING_SOON => IntegrationEvents.Events.MovieStatus.COMMING_SOON,
                MovieStatus.NOW_SHOWING => IntegrationEvents.Events.MovieStatus.NOW_SHOWING,
                MovieStatus.ENDED => IntegrationEvents.Events.MovieStatus.ENDED,
                _ => throw new NotImplementedException(),
            }
        );

        integrationEventService.Add(integrationEvent);

        return true;
    }
}