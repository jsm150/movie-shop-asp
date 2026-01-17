using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;

namespace Screening.Infrastructure.Configurations;

public class MovieEntityTypeConfiguration : IEntityTypeConfiguration<MovieEntity>
{
    public void Configure(EntityTypeBuilder<MovieEntity> builder)
    {
        builder.ToTable("Movies", "Screening");

        builder.HasKey(x => x.MovieId);

        builder.Property(x => x.MovieId)
            .ValueGeneratedNever();

        builder.Property(x => x.MovieStatus)
            .HasConversion<string>()
            .IsRequired();
    }
}
