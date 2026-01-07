using Microsoft.EntityFrameworkCore;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using Movie.Infrastructure;
using Movie.Infrastructure.Repositories;
using movie_shop_asp.Server.ExceptionHandler;
using movie_shop_asp.Server.Movie.API.Application.Behaviors;


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


                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<Program>();

                    cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
                });

                services.AddValidatorsFromAssemblyContaining<Program>();

                services.AddExceptionHandler<ValidateExceptionHandler>();
                services.AddExceptionHandler<DomainExceptionHandler<MovieDomainException>>();

                services.AddScoped<IMovieRepository, MovieRepository>();
            }
        }
    }
}
