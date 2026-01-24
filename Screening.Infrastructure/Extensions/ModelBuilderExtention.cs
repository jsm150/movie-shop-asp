using Microsoft.EntityFrameworkCore;
using Screening.Infrastructure.Configurations;


namespace Screening.Infrastructure.Extensions;

public static class ModelBuilderExtention
{
    extension(ModelBuilder modelBuilder)
    {
        public void ApplyScreeningModuleConfigurations()
        {
            modelBuilder.ApplyConfiguration(new MovieEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TheaterEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ScreenEntityTypeConfiguration());
        }
    }
}
