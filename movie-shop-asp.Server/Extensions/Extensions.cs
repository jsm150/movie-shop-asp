using FluentValidation;
using IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Movie.API.Controllers;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;
using Movie.Infrastructure;
using Movie.Infrastructure.Repositories;
using movie_shop_asp.Server.Application.Behaviors;
using movie_shop_asp.Server.Application.ExceptionHandler;
using movie_shop_asp.Server.Infrastructure;
using Screening.API.Application.IntegrationEventHandler;
using Screening.Domain.Exceptions;
using Screening.Infrastructure;
using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Infrastructure.Repositories;
using Screening.Domain.Aggregate.ScreenAggregate;


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
                    .AddScoped<IScreeningContext>(provider => provider.GetRequiredService<MovieShopContext>());

                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<Program>();
                    cfg.RegisterServicesFromAssemblyContaining<MovieController>(); // Movie
                    cfg.RegisterServicesFromAssemblyContaining<MovieCreatedIntegrationEventHandler>(); // Screening

                    cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
                    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
                });

                services.AddValidatorsFromAssemblyContaining<Program>();

                services.AddExceptionHandler<ValidateExceptionHandler>();
                services.AddExceptionHandler<DomainExceptionHandler<MovieDomainException>>();
                services.AddExceptionHandler<DomainExceptionHandler<ScreeningDomainException>>();

                services.AddScoped<InProcessIntegrationEventService>();

                services.AddScoped<IMovieRepository, Movie.Infrastructure.Repositories.MovieRepository>();
                services.AddScoped<Screening.Domain.Aggregate.MovieAggregate.IMovieRepository, Screening.Infrastructure.Repositories.MovieRepository>();
                services.AddScoped<ITheaterRepository, TheaterRepository>();
                services.AddScoped<IScreenRepository, ScreenRepository>();
            }
        }
    }
}
