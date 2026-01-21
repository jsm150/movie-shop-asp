using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.ScreenAggregate;

namespace Screening.API.Infrastructure;

public interface IScreeningContext
{
    DbSet<Domain.Aggregate.MovieAggregate.Movie> ScreeningMovies { get; set; }
    DbSet<Domain.Aggregate.TheaterAggregate.Theater> Theaters { get; set; } 
    DbSet<Screen> Screens { get; set; }
    Task SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
