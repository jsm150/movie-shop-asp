using MediatR;
using Movie.IntegrationEvent;
using Screening.Domain.Aggregate.MovieAggregate;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieCreatedIntegrationEventHandler(IMovieRepository movieRepository) : INotificationHandler<MovieCreatedIntegrationEvent>
{
    public async Task Handle(MovieCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var movie = new MovieEntity()
        {
            MovieId = @event.MovieId,
            MovieStatus = @event.MovieStatus switch
            {
                Movie.IntegrationEvent.MovieStatus.PREPARING => Domain.Aggregate.MovieAggregate.MovieStatus.PREPARING,
                Movie.IntegrationEvent.MovieStatus.COMMING_SOON => Domain.Aggregate.MovieAggregate.MovieStatus.COMMING_SOON,
                Movie.IntegrationEvent.MovieStatus.NOW_SHOWING => Domain.Aggregate.MovieAggregate.MovieStatus.NOW_SHOWING,
                Movie.IntegrationEvent.MovieStatus.ENDED => Domain.Aggregate.MovieAggregate.MovieStatus.ENDED,
                _ => throw new NotImplementedException(),
            }
        };

        movieRepository.Add(movie);
        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
