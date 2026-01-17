using IntegrationEvents;
using IntegrationEvents.Events;
using MediatR;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using movie_shop_asp.Server.Movie.API.Application.Commands;

namespace Movie.API.Application.Commands;

public class DeleteMovieCommandHandler(IMovieRepository movieRepository, InProcessIntegrationEventService integrationEventService) : IRequestHandler<DeleteMovieCommand, bool>
{
    public async Task<bool> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await movieRepository.GetAsync(request.MovieId) 
            ?? throw new MovieDomainException("대상 영화를 찾을 수 없습니다.");

        if (!movie.CanRemove())
        {
            throw new MovieDomainException("상영중에는 영화를 삭제할 수 없습니다.");
        }

        movieRepository.Delete(movie);
        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var integrationEvent = new MovieDeletedIntegrationEvent
        {
            MovieId = request.MovieId
        };

        integrationEventService.Add(integrationEvent);

        return true;
    }
}