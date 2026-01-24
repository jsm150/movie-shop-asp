using BuildingBlocks.Domain;
using Screening.Domain.Aggregate.MovieAggregate;

namespace Screening.Infrastructure.Repository;

public class ScreeningMovieRepository(IScreeningContext context) : IScreeningMovieRepository
{
    public IUnitOfWork UnitOfWork => context;

    public void Add(Movie value)
    {
        context.ScreeningMovies.Add(value);
    }

    public async Task<Movie?> FindAsync(long id)
    {
        return await context.ScreeningMovies.FindAsync(id);
    }

    public void Remove(Movie value)
    {
        context.ScreeningMovies.Remove(value);
    }
}
