using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;

namespace Screening.Infrastructure.Configurations;

public class ScreenEntityTypeConfiguration : IEntityTypeConfiguration<Screen>
{
    public void Configure(EntityTypeBuilder<Screen> builder)
    {
        builder.ToTable("Screens", "Screening");

        builder.HasKey(x => x.ScreenId);

        builder.Property(x => x.ScreenId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.MovieId)
            .IsRequired();

        builder.Property(x => x.TheaterId)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.SalesStartAt)
            .IsRequired();

        builder.Property(x => x.SalesEndAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(x => x.TheaterId);
        builder.HasIndex(x => x.MovieId);

        // (선택) 동일 상영관에서 동일 시작시간 중복 방지 - 정책이면 켜기
        // builder.HasIndex(x => new { x.TheaterId, x.StartTime }).IsUnique();

        // SeatHolds 매핑 (Screen이 소유)
        builder.OwnsMany(x => x.SeatHolds, holds =>
        {
            holds.ToTable("SeatHolds", "Screening");

            holds.WithOwner()
                .HasForeignKey(x => x.ScreenId);

            // 좌석 점유 단위 키: ScreenId + SeatCode
            holds.HasKey(x => new { x.ScreenId, x.SeatCode });

            holds.Property(x => x.SeatCode)
                .HasConversion(x => x.Value, v => new SeatCode(v))
                .HasMaxLength(10)
                .IsRequired();

            holds.Property(x => x.HoldToken)
                .IsRequired();

            holds.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            holds.Property(x => x.HeldUntil);

            holds.HasIndex(x => x.HoldToken);
            holds.HasIndex(x => new { x.ScreenId, x.Status });
            holds.HasIndex(x => new { x.Status, x.HeldUntil });
        });

        builder.Navigation(x => x.SeatHolds)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}