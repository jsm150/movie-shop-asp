using Microsoft.EntityFrameworkCore;
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
                    
                AddDbContext(services, builder.Configuration.GetConnectionString("DefaultConnection"));
            }
        }

        private static void AddDbContext(IServiceCollection services, string? connection) {
            services.AddDbContext<MovieContext>(options =>
            {
                options.UseNpgsql(connection);
            });
        }
    }
}
