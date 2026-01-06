using Microsoft.EntityFrameworkCore;
using Movie.Domain.Aggregate;
using Movie.Infrastructure;
using Movie.Infrastructure.Repositories;


namespace movie_shop_asp.Server.Extensions
{
    public static class Extensions
    {
        extension(IHostApplicationBuilder builder)
        {
            public void AddApplicationServices()
            {
                var services = builder.Services;
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<MovieContext>(options =>
                {
                    options.UseNpgsql(connectionString, b =>
                    {
                        b.MigrationsAssembly(typeof(MovieContext).Assembly.GetName().Name);
                        b.MigrationsHistoryTable("__EFMigrationsHistory", "Movie");
                    });
                });



                services.AddScoped<IMovieRepository, MovieRepository>();
            }
        }
    }
}
