using FluentAssertions;
using NSubstitute;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Domain.Exceptions;

namespace Screening.UnitTests.Domain.ScreenAggregate;

public class ScreenTests
{
    private readonly IScreenRepository _repository;
    private readonly Theater _activeTheater;

    public ScreenTests()
    {
        _repository = Substitute.For<IScreenRepository>();
        _repository.HasConflict(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(false);
        _activeTheater = new Theater(1, [new SeatCode("A1")]);
    }

    [Fact]
    public async Task CreateAsync_WhenEndTimeBeforeStartTime_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };

        var act = () => Screen.CreateAsync(
            movie,
            _activeTheater,
            _repository,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        await act.Should().ThrowAsync<ScreeningDomainException>();
    }

    [Fact]
    public async Task CreateAsync_WhenMovieCannotBeScreened_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.PREPARING };

        var act = () => Screen.CreateAsync(
            movie,
            _activeTheater,
            _repository,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        await act.Should().ThrowAsync<ScreeningDomainException>();
    }

    [Fact]
    public async Task CreateAsync_WhenScreeningTimeConflicts_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var repository = Substitute.For<IScreenRepository>();
        repository.HasConflict(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(true);

        var act = () => Screen.CreateAsync(
            movie,
            _activeTheater,
            repository,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        await act.Should().ThrowAsync<ScreeningDomainException>();
    }

    [Fact]
    public async Task CreateAsync_WhenTheaterIsNotActive_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var inactiveTheater = CreateInactiveTheater(1, [new SeatCode("A1")]);

        var act = () => Screen.CreateAsync(
            movie,
            inactiveTheater,
            _repository,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        await act.Should().ThrowAsync<ScreeningDomainException>()
            .WithMessage("*활성화된 상영관*");
    }

    [Fact]
    public async Task HoldSeats_WhenNotOnSale_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var screen = await Screen.CreateAsync(
            movie,
            _activeTheater,
            _repository,
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
    public async Task ConfirmSeats_WhenTokenNotFound_ShouldThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var screen = await Screen.CreateAsync(
            movie,
            _activeTheater,
            _repository,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        var act = () => screen.ConfirmSeats(Guid.NewGuid(), DateTimeOffset.UtcNow);

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public async Task ReleaseSeats_WhenNoSeats_ShouldNotThrow()
    {
        var movie = new Movie { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        var screen = await Screen.CreateAsync(
            movie,
            _activeTheater,
            _repository,
            startTime: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            salesStartAt: new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            salesEndAt: new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero));

        var act = () => screen.ReleaseSeats(Guid.NewGuid());

        act.Should().NotThrow();
    }

    private static Theater CreateInactiveTheater(long theaterId, IReadOnlyCollection<SeatCode> seatCodes)
    {
        var theater = new Theater(theaterId, seatCodes);
        // IsActive를 false로 설정하기 위해 리플렉션 사용
        var isActiveProperty = typeof(Theater).GetProperty(nameof(Theater.IsActive));
        isActiveProperty!.SetValue(theater, false);
        return theater;
    }
}