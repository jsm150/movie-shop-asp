using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie.API.Infrastructure.Configurations;
using Movie.Domain.Aggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.API.Infrastructure.Configurations
{
    internal static class NavigationBuilderExtension
    {
        extension(OwnedNavigationBuilder<Domain.Aggregate.Movie, MovieInfo> builder)
        {
            public void ConfigureMovieInfo()
            {
                builder.Property(x => x.AdienceRating)
                    .HasConversion<string>();

                builder.HasIndex(x => x.Title).IsUnique();

                builder.OwnsMany(x => x.Casts);
            }
        }
    }
}
