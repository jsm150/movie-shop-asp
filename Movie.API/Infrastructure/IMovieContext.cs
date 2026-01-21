using Microsoft.EntityFrameworkCore;

namespace Movie.API.Infrastructure;

public interface IMovieContext
{
    DbSet<Domain.Aggregate.Movie> Movies { get; set; }
    Task SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
