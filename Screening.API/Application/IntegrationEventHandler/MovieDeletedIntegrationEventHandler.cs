using MediatR;
using Movie.IntegrationEvent;
using Screening.API.Infrastructure;
using Screening.Domain.Aggregate.MovieAggregate;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieDeletedIntegrationEventHandler(IScreeningContext context) : INotificationHandler<MovieDeletedIntegrationEvent>
{
    public async Task Handle(MovieDeletedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        MovieEntity movie = (await context.ScreeningMovies.FindAsync(@event.MovieId))!;
        context.ScreeningMovies.Remove(movie);
        await context.SaveEntitiesAsync(cancellationToken);
    }
}
