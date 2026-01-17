using IntegrationEvents;
using IntegrationEvents.Events;
using MediatR;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using MovieEntity = Movie.Domain.Aggregate.Movie;

namespace Movie.API.Application.Commands;

public class RegisterMovieCommandHandler(IMovieRepository movieRepository, InProcessIntegrationEventService integrationEventService) 
    : IRequestHandler<RegisterMovieCommand, bool>
{

    public async Task<bool> Handle(RegisterMovieCommand request, CancellationToken cancellationToken)
    {
        if (await movieRepository.ExistsByTitle(request.Title))
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

        movieRepository.Add(movie);
        await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var integrationEvent = new MovieCreatedIntegrationEvent
        {
            MovieId = movie.MovieId,
            MovieStatus = movie.MovieStatus switch
            {
                Domain.Aggregate.MovieStatus.PREPARING => IntegrationEvents.Events.MovieStatus.PREPARING,
                Domain.Aggregate.MovieStatus.COMMING_SOON => IntegrationEvents.Events.MovieStatus.COMMING_SOON,
                Domain.Aggregate.MovieStatus.NOW_SHOWING => IntegrationEvents.Events.MovieStatus.NOW_SHOWING,
                Domain.Aggregate.MovieStatus.ENDED => IntegrationEvents.Events.MovieStatus.ENDED,
                _ => throw new NotImplementedException(),
            }
        };

        integrationEventService.Add(integrationEvent);

        return true;
    }
}

