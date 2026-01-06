using Microsoft.EntityFrameworkCore;
using Movie.Domain.Aggregate;
using Movie.Infrastructure.EntityConfigurations;
using SeedWork.Domain;

namespace Movie.Infrastructure
{
    public class MovieContext : DbContext, IUnitOfWork
    {
        public MovieContext(DbContextOptions<MovieContext> options) : base(options) { }
        

        public DbSet<Domain.Aggregate.Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Movie"); 
            modelBuilder.ApplyConfiguration(new MovieEntityTypeConfiguration());
        }


    }
}
