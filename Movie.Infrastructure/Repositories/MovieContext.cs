using Microsoft.EntityFrameworkCore;
using Movie.Domain.Aggregate;
using Movie.Infrastructure.EntityConfigurations;

namespace Movie.Infrastructure.Repositories
{
    public class MovieContext : DbContext
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
