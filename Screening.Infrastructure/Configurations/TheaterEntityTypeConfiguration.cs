using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Screening.Domain.Aggregate.TheaterAggregate;
using TheaterEntity = Screening.Domain.Aggregate.TheaterAggregate.Theater;

namespace Screening.Infrastructure.Configurations;

public class TheaterEntityTypeConfiguration : IEntityTypeConfiguration<TheaterEntity>
{
    public void Configure(EntityTypeBuilder<TheaterEntity> builder)
    {
        builder.ToTable("Theaters", "Screening");

        builder.HasKey(x => x.TheaterId);

        builder.Property(x => x.TheaterId)
            .ValueGeneratedNever();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

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