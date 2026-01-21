using MediatR;
using Movie.IntegrationEvent;
using Screening.Domain.Aggregate.MovieAggregate;
using MovieStatus = Screening.Domain.Aggregate.MovieAggregate.MovieStatus;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieStatusChangedIntegrationEventHandler(IMovieRepository movieRepository) : INotificationHandler<MovieStatusChangedIntegrationEvent>
{
    public async Task Handle(MovieStatusChangedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        MovieEntity movie = (await movieRepository.GetAsync(@event.MovieId))!;
        movie.MovieStatus = @event.MovieStatus switch
        {
            Movie.IntegrationEvent.MovieStatus.PREPARING => MovieStatus.PREPARING,
            Movie.IntegrationEvent.MovieStatus.COMMING_SOON => MovieStatus.COMMING_SOON,
            Movie.IntegrationEvent.MovieStatus.NOW_SHOWING => MovieStatus.NOW_SHOWING,
            Movie.IntegrationEvent.MovieStatus.ENDED => MovieStatus.ENDED,
            _ => movie.MovieStatus
        };

        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
