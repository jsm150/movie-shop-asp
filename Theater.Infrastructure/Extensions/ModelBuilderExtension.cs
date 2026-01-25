using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Theater.Infrastructure.Configurations;

namespace Theater.Infrastructure.Extensions;

public static class ModelBuilderExtension
{
    extension(ModelBuilder modelBuilder)
    {
        public void ApplyTheaterModuleConfigurations()
        {
            modelBuilder.ApplyConfiguration(new TheaterEntityTypeConfiguration());
        }
    }
}
