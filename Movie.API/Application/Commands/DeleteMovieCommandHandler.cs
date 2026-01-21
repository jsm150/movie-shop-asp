using MediatR;
using Movie.API.Infrastructure;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using Movie.IntegrationEvent;

namespace Movie.API.Application.Commands;

public class DeleteMovieCommandHandler(
    IMovieContext context, 
    IMediator mediator
) 
    : IRequestHandler<DeleteMovieCommand, bool>
{
    public async Task<bool> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await context.Movies.FindAsync(request.MovieId) 
            ?? throw new MovieDomainException("대상 영화를 찾을 수 없습니다.");

        if (!movie.CanRemove())
        {
            throw new MovieDomainException("상영중에는 영화를 삭제할 수 없습니다.");
        }

        context.Movies.Remove(movie);

        var integrationEvent = new MovieDeletedIntegrationEvent
        {
            MovieId = request.MovieId
        };

        await mediator.Publish(integrationEvent, cancellationToken);

        return true;
    }
}