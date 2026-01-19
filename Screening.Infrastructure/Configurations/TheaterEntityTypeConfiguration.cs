using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;

namespace Screening.Infrastructure.Configurations;

public class TheaterEntityTypeConfiguration : IEntityTypeConfiguration<Theater>
{
    public void Configure(EntityTypeBuilder<Theater> builder)
    {
        builder.ToTable("Theaters", "Screening");

        builder.HasKey(x => x.TheaterId);

        builder.Property(x => x.TheaterId)
            .ValueGeneratedNever();

        builder.OwnsMany(x => x.Seats, seats =>
        {
            seats.ToTable("TheaterSeats", "Screening");

            seats.WithOwner()
                .HasForeignKey(x => x.TheaterId);

            seats.HasKey(x => new { x.TheaterId, x.SeatCode });

            seats.Property(x => x.SeatCode)
                .HasConversion(x => x.Value, v => new SeatCode(v))
                .HasMaxLength(10)
                .IsRequired();
        });

        builder.Navigation(x => x.Seats).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}