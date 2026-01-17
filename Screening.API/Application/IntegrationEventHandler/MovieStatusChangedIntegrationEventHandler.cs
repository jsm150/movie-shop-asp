using IntegrationEvents.Events;
using MediatR;
using Screening.Domain.Aggregate.MovieAggregate;
using MovieStatus = Screening.Domain.Aggregate.MovieAggregate.MovieStatus;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieStatusChangedIntegrationEventHandler(IMovieRepository movieRepository) : INotificationHandler<MovieStatusChangedIntegrationEvent>
{
    public async Task Handle(MovieStatusChangedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        Movie movie = (await movieRepository.GetAsync(@event.MovieId))!;
        movie.MovieStatus = @event.MovieStatus switch
        {
            IntegrationEvents.Events.MovieStatus.PREPARING => MovieStatus.PREPARING,
            IntegrationEvents.Events.MovieStatus.COMMING_SOON => MovieStatus.COMMING_SOON,
            IntegrationEvents.Events.MovieStatus.NOW_SHOWING => MovieStatus.NOW_SHOWING,
            IntegrationEvents.Events.MovieStatus.ENDED => MovieStatus.ENDED,
            _ => movie.MovieStatus
        };

        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
