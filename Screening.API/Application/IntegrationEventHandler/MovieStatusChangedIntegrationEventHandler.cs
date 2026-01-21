using MediatR;
using Movie.IntegrationEvent;
using Screening.Domain.Aggregate.MovieAggregate;
using MovieStatus = Screening.Domain.Aggregate.MovieAggregate.MovieStatus;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;
using Screening.API.Infrastructure;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieStatusChangedIntegrationEventHandler(IScreeningContext context) : INotificationHandler<MovieStatusChangedIntegrationEvent>
{
    public async Task Handle(MovieStatusChangedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        MovieEntity movie = (await context.ScreeningMovies.FindAsync(@event.MovieId))!;
        movie.MovieStatus = @event.MovieStatus switch
        {
            Movie.IntegrationEvent.MovieStatus.PREPARING => MovieStatus.PREPARING,
            Movie.IntegrationEvent.MovieStatus.COMMING_SOON => MovieStatus.COMMING_SOON,
            Movie.IntegrationEvent.MovieStatus.NOW_SHOWING => MovieStatus.NOW_SHOWING,
            Movie.IntegrationEvent.MovieStatus.ENDED => MovieStatus.ENDED,
            _ => movie.MovieStatus
        };

        await context.SaveEntitiesAsync(cancellationToken);
    }
}
