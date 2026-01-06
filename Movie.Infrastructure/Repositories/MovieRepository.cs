using Movie.Domain.Aggregate;
using SeedWork.Domain;
using System;
using System.Collections.Generic;
using System.Text;


namespace Movie.Infrastructure.Repositories
{
    public class MovieRepository(MovieContext context) : IMovieRepository
    {
        public IUnitOfWork UnitOfWork => context;

        public Domain.Aggregate.Movie Add(Domain.Aggregate.Movie movie)
        {
            return context.Movies.Add(movie).Entity;
        }

        public async Task<Domain.Aggregate.Movie?> GetAsync(long movieId)
        {
            return await context.Movies.FindAsync(movieId);
        }
    }
}
