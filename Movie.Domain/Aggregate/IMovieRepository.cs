using SeedWork.Domain;

namespace Movie.Domain.Aggregate
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Movie Add(Movie movie);
        void Delete(Movie movie);
        Task<Movie?> GetAsync(long movieId);
        Task<bool> ExistsByTitle(string title);
    }
}
