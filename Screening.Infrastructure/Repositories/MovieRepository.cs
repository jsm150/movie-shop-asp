using BuildingBlocks.Domain;
using Screening.Domain.Aggregate.MovieAggregate;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;

namespace Screening.Infrastructure.Repositories;

public class MovieRepository(IMovieContext context) : IMovieRepository
{
    public IUnitOfWork UnitOfWork => context;

    public MovieEntity Add(MovieEntity movie)
    {
        return context.ScreeningMovies.Add(movie).Entity;
    }

    public async Task<MovieEntity?> GetAsync(long movieId)
    {
        return await context.ScreeningMovies.FindAsync(movieId);
    }
}
