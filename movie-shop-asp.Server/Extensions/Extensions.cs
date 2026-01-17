using Microsoft.EntityFrameworkCore;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using Movie.Infrastructure;
using Movie.Infrastructure.Repositories;
using movie_shop_asp.Server.Application.Behaviors;
using movie_shop_asp.Server.Application.ExceptionHandler;
using movie_shop_asp.Server.Infrastructure;


namespace movie_shop_asp.Server.Extensions
{
    public static class Extensions
    {
        extension(IHostApplicationBuilder builder)
        {
            public void AddApplicationServices()
            {
                var services = builder.Services;
                var host = $"Host={builder.Configuration.GetConnectionString("Host")};";
                var connectionString = host + builder.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<MovieShopContext>(options =>
                {
                    options.UseNpgsql(connectionString, b =>
                    {
                        b.MigrationsAssembly(typeof(MovieShopContext).Assembly.GetName().Name);
                        b.MigrationsHistoryTable("__EFMigrationsHistory", "public");
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
                services.AddScoped<IMovieContext>(provider => provider.GetRequiredService<MovieShopContext>());
            }
        }
    }
}
