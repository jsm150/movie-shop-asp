using MediatR;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;

namespace movie_shop_asp.Server.Movie.API.Application.Commands;

public class UpdateMovieCommandHandler(IMovieRepository movieRepository) : IRequestHandler<UpdateMovieCommand, bool>
{
    public async Task<bool> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await movieRepository.GetAsync(request.MovieId) 
            ?? throw new MovieDomainException("대상 영화를 찾을 수 없습니다.");

        var newInfo = new MovieInfo
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
        };

        movie.UpdateMovieInfo(newInfo);

        return (await movieRepository.UnitOfWork.SaveChangesAsync(cancellationToken)) > 0;
    }
}