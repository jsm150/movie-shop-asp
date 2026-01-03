using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie.Domain.Aggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.Infrastructure.Configurations
{
    internal static class NavigationBuilderExtension
    {
        extension(OwnedNavigationBuilder<Domain.Aggregate.Movie, MovieInfo> builder)
        {
            public void ConfigureMovieInfo()
            {
                builder.Property(x => x.AdienceRating)
                    .HasConversion<string>();

                builder.OwnsMany(x => x.Casts);
            }
        }
    }
}
