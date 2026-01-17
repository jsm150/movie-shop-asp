using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;

namespace Screening.Infrastructure;

public interface IMovieContext : IUnitOfWork
{
    DbSet<MovieEntity> ScreeningMovies { get; set; }
}
