using IntegrationEvents.Events;
using MediatR;
using Screening.Domain.Aggregate.MovieAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Screening.API.Application.IntegrationEventHandler;

public class MovieDeletedIntegrationEventHandler(IMovieRepository movieRepository) : INotificationHandler<MovieDeletedIntegrationEvent>
{
    public async Task Handle(MovieDeletedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        Movie movie = (await movieRepository.GetAsync(@event.MovieId))!;
        movieRepository.Remove(movie);
        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
