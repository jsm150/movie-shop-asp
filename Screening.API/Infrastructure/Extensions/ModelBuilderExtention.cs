using Microsoft.EntityFrameworkCore;
using Screening.API.Infrastructure.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Screening.API.Infrastructure.Extensions;

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
