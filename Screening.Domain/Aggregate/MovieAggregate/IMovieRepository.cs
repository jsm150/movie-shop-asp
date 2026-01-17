using BuildingBlocks.Domain;


namespace Screening.Domain.Aggregate.MovieAggregate;

public interface IMovieRepository : IRepository<Movie>
{
    public Movie Add(Movie movie);
    public Task<Movie?> GetAsync(long movieId);
    public void Remove(Movie movie);
}
