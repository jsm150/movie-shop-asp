using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Domain;


namespace Movie.Infrastructure;

public interface IMovieContext : IUnitOfWork
{
    DbSet<Domain.Aggregate.Movie> Movies { get; set; }
}
