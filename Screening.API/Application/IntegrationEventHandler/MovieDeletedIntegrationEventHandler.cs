using MediatR;
using Movie.IntegrationEvent;
using Screening.Domain.Aggregate.MovieAggregate;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieDeletedIntegrationEventHandler(IMovieRepository movieRepository) : INotificationHandler<MovieDeletedIntegrationEvent>
{
    public async Task Handle(MovieDeletedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        MovieEntity movie = (await movieRepository.GetAsync(@event.MovieId))!;
        movieRepository.Remove(movie);
        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
