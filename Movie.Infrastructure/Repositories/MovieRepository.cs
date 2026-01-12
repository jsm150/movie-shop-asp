using Microsoft.EntityFrameworkCore;
using Movie.Domain.Aggregate;
using SeedWork.Domain;

namespace Movie.Infrastructure.Repositories
{
    public class MovieRepository(MovieContext context) : IMovieRepository
    {
        public IUnitOfWork UnitOfWork => context;

        public Domain.Aggregate.Movie Add(Domain.Aggregate.Movie movie)
        {
            return context.Movies.Add(movie).Entity;
        }

        public void Delete(Domain.Aggregate.Movie movie)
        {
            context.Movies.Remove(movie);
        }

        public async Task<bool> ExistsByTitle(string title)
        {
            return await context.Movies.AnyAsync(m => m.MovieInfo.Title == title);
        }

        public async Task<Domain.Aggregate.Movie?> GetAsync(long movieId)
        {
            return await context.Movies.FindAsync(movieId);
        }
    }
}
