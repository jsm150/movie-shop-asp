using MediatR;
using Microsoft.EntityFrameworkCore;
using Movie.API.Infrastructure;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using Movie.IntegrationEvent;
using MovieEntity = Movie.Domain.Aggregate.Movie;

namespace Movie.API.Application.Commands;

public class RegisterMovieCommandHandler(
    IMovieContext context, 
    IMediator mediator
) 
    : IRequestHandler<RegisterMovieCommand, bool>
{

    public async Task<bool> Handle(RegisterMovieCommand request, CancellationToken cancellationToken)
    {
        if (await context.Movies.AnyAsync(m => m.MovieInfo.Title == request.Title))
        {
            throw new MovieDomainException($"'{request.Title}' 제목을 가진 영화가 이미 존재합니다.");
        }

        var movie = new MovieEntity
        {
            MovieInfo = new MovieInfo
            {
                Title = request.Title,
                Director = request.Director,
                Genres = request.Genres.ToList(),
                RuntimeMinutes = request.RuntimeMinutes,
                AdienceRating = request.AdienceRating,
                Synopsis = request.Synopsis,
                ReleaseDate = request.ReleaseDate.UtcDateTime,
                Casts = request.Casts.Select(c => new Actor
                {
                    Name = c.Name,
                    Role = c.Role,
                    DateOfBirth = c.DateOfBirth.UtcDateTime,
                    National = c.National
                }).ToList()
            }
        };

        context.Movies.Add(movie);

        await context.SaveEntitiesAsync(cancellationToken);

        var integrationEvent = new MovieCreatedIntegrationEvent
        {
            MovieId = movie.MovieId,
            MovieStatus = movie.MovieStatus switch
            {
                Domain.Aggregate.MovieStatus.PREPARING => IntegrationEvent.MovieStatus.PREPARING,
                Domain.Aggregate.MovieStatus.COMMING_SOON => IntegrationEvent.MovieStatus.COMMING_SOON,
                Domain.Aggregate.MovieStatus.NOW_SHOWING => IntegrationEvent.MovieStatus.NOW_SHOWING,
                Domain.Aggregate.MovieStatus.ENDED => IntegrationEvent.MovieStatus.ENDED,
                _ => throw new NotImplementedException(),
            }
        };

        await mediator.Publish(integrationEvent, cancellationToken);

        return true;
    }
}

