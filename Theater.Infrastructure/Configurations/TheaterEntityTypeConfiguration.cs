using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Theater.Domain.Aggregate;

namespace Theater.Infrastructure.Configurations;

public class TheaterEntityTypeConfiguration : IEntityTypeConfiguration<Domain.Aggregate.Theater>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregate.Theater> builder)
    {
        builder.ToTable("Theaters");

        // Primary Key
        builder.HasKey(t => t.TheaterId);
        builder.Property(t => t.TheaterId)
            .ValueGeneratedOnAdd();

        // 기본 정보
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Floor)
            .IsRequired();

        // 상영관 타입 (enum -> string 변환)
        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // 상영관 크기
        builder.Property(t => t.RowCount)
            .IsRequired();

        builder.Property(t => t.ColumnCount)
            .IsRequired();

        // 운영 상태
        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // TotalSeats는 계산 속성이므로 매핑에서 제외
        builder.Ignore(t => t.TotalSeats);

        // TheaterSeat를 Owned Entity로 설정
        builder.OwnsMany(t => t.Seats, seatBuilder =>
        {
            seatBuilder.ToTable("TheaterSeats");

            seatBuilder.WithOwner()
                .HasForeignKey(s => s.TheaterId);

            seatBuilder.HasKey(s => new { s.TheaterId, s.SeatCode });

            seatBuilder.Property(s => s.TheaterId)
                .IsRequired();

            // Value Object (SeatCode) 매핑
            seatBuilder.Property(x => x.SeatCode)
                .HasConversion(x => x.Value, v => new SeatCode(v))
                .HasMaxLength(10)
                .IsRequired();
        });

        // Navigation property를 backing field에 매핑
        builder.Navigation(t => t.Seats)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
