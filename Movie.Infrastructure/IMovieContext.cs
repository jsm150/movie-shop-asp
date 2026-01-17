using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;


namespace Movie.Infrastructure;

public interface IMovieContext : IUnitOfWork
{
    DbSet<Domain.Aggregate.Movie> Movies { get; set; }
}
