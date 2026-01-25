using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;


namespace Screening.Infrastructure;

public interface IScreeningContext : IUnitOfWork
{
    DbSet<Movie> ScreeningMovies { get; set; }
    DbSet<Theater> ScreeingTheaters { get; set; } 
    DbSet<Screen> Screens { get; set; }
}
