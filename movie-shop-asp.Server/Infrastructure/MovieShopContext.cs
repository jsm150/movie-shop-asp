using Microsoft.EntityFrameworkCore;
using Movie.Infrastructure;
using Movie.Infrastructure.Configurations;
using Shared.Kernel.Domain;
using MovieEntity = Movie.Domain.Aggregate.Movie;

namespace movie_shop_asp.Server.Infrastructure
{
    public class MovieShopContext(DbContextOptions<MovieShopContext> options) : 
        DbContext(options),
        IMovieContext
    {
        public DbSet<MovieEntity> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MovieEntityTypeConfiguration());
        }
    }
}
