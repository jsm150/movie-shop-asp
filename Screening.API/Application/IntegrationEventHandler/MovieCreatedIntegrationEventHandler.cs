using IntegrationEvents.Events;
using MediatR;
using Screening.Domain.Aggregate.MovieAggregate;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieCreatedIntegrationEventHandler(IMovieRepository movieRepository) : INotificationHandler<MovieCreatedIntegrationEvent>
{
    public async Task Handle(MovieCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var movie = new Movie()
        {
            MovieId = @event.MovieId,
            MovieStatus = @event.MovieStatus switch
            {
                IntegrationEvents.Events.MovieStatus.PREPARING => Domain.Aggregate.MovieAggregate.MovieStatus.PREPARING,
                IntegrationEvents.Events.MovieStatus.COMMING_SOON => Domain.Aggregate.MovieAggregate.MovieStatus.COMMING_SOON,
                IntegrationEvents.Events.MovieStatus.NOW_SHOWING => Domain.Aggregate.MovieAggregate.MovieStatus.NOW_SHOWING,
                IntegrationEvents.Events.MovieStatus.ENDED => Domain.Aggregate.MovieAggregate.MovieStatus.ENDED,
                _ => throw new NotImplementedException(),
            }
        };

        movieRepository.Add(movie);
        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
