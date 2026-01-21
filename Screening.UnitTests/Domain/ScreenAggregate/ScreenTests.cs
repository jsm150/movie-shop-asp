using FluentAssertions;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Domain.Exceptions;

namespace Screening.UnitTests.Domain.ScreenAggregate;

public class ScreenTests
{
    [Fact]
    public void Ctor_WhenEndTimeBeforeStartTime_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };

        var act = () => _ = new Screen(
            movie,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void Ctor_WhenMovieCannotBeScreened_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.PREPARING };

        var act = () => _ = new Screen(
            movie,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void HoldSeats_WhenNotOnSale_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var screen = new Screen(
            movie,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        var theater = new Theater(1, [new SeatCode("A1")]);

        var act = () => screen.HoldSeats(
            theater,
            seatCodes: ["A1"],
            holdToken: Guid.NewGuid(),
            heldUntil: new DateTimeOffset(2026, 1, 1, 8, 10, 0, TimeSpan.Zero),
            now: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero));

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void ConfirmSeats_WhenTokenNotFound_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var screen = new Screen(
            movie,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        var act = () => screen.ConfirmSeats(Guid.NewGuid(), DateTimeOffset.UtcNow);

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void ReleaseSeats_WhenNoSeats_ShouldNotThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var screen = new Screen(
            movie,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        var act = () => screen.ReleaseSeats(Guid.NewGuid());

        act.Should().NotThrow();
    }
}