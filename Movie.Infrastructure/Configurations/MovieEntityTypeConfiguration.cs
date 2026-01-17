using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movie.Domain.Aggregate;
using Movie.Infrastructure.Configurations;


namespace Movie.Infrastructure.Configurations;

public class MovieEntityTypeConfiguration : IEntityTypeConfiguration<Domain.Aggregate.Movie>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregate.Movie> builder)
    {
        builder.ToTable("Movies", "Movie");

        builder.HasKey(x => x.MovieId);

        builder.OwnsOne(x => x.MovieInfo,
            mi => mi.ConfigureMovieInfo()
        );

        builder.Property(x => x.MovieStatus)
            .HasConversion<string>();

    }
}

