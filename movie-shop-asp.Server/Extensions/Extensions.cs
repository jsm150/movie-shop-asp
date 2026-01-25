using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Movie.API.Controllers;
using Movie.API.Infrastructure;
using Movie.Domain.Exceptions;
using movie_shop_asp.Server.Application.Behaviors;
using movie_shop_asp.Server.Application.ExceptionHandler;
using movie_shop_asp.Server.Infrastructure;
using Screening.API.Controllers;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Exceptions;
using Screening.Infrastructure;
using Screening.Infrastructure.Repository;
using Theater.API.Controllers;
using Theater.Domain.Exceptions;
using Theater.Infrastructure;



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
                })
                    .AddScoped<IMovieContext>(provider => provider.GetRequiredService<MovieShopContext>())
                    .AddScoped<IScreeningContext>(provider => provider.GetRequiredService<MovieShopContext>())
                    .AddScoped<ITheaterContext>(provider => provider.GetRequiredService<MovieShopContext>());


                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<Program>();
                    cfg.RegisterServicesFromAssemblyContaining<MovieController>(); // Movie
                    cfg.RegisterServicesFromAssemblyContaining<ScreenController>(); // Screening
                    cfg.RegisterServicesFromAssemblyContaining<TheaterController>(); // Theater

                    cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
                    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
                });

                services.AddValidatorsFromAssemblyContaining<Program>();

                services.AddExceptionHandler<ValidateExceptionHandler>();
                services.AddExceptionHandler<DomainExceptionHandler<MovieDomainException>>();
                services.AddExceptionHandler<DomainExceptionHandler<ScreeningDomainException>>();
                services.AddExceptionHandler<DomainExceptionHandler<TheaterDomainException>>();

                services.AddScoped<IScreenRepository, ScreenRepository>();
                services.AddScoped<IScreeningMovieRepository, ScreeningMovieRepository>();
                services.AddScoped<
                    Screening.Domain.Aggregate.TheaterAggregate.ITheaterRepository, 
                    Screening.Infrastructure.Repository.TheaterRepository>();

                services.AddScoped<
                    Theater.Domain.Aggregate.ITheaterRepository, 
                    Theater.Infrastructure.Repositories.TheaterRepository>();
            }
        }
    }
}
