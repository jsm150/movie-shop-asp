using Microsoft.EntityFrameworkCore;
using Movie.API.Infrastructure.Configurations;


namespace Movie.API.Infrastructure.Extensions;

public static class ModelBuilderExtension
{
    extension(ModelBuilder modelBuilder)
    {
        public void ApplyMovieModuleConfigurations()
        {
            modelBuilder.ApplyConfiguration(new MovieEntityTypeConfiguration());
        }
    }
}
