using SeedWork.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.Domain.Aggregate
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Movie Add(Movie movie);
        Task<Movie?> GetAsync(long movieId);
        Task<bool> ExistsByTitle(string title);
    }
}
