using FluentAssertions;
using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Domain.Exceptions;

namespace Screening.UnitTests.Domain.TheaterAggregate;

public class TheaterTests
{
    [Fact]
    public void NormalizeAndValidateSeatCodes_WhenEmpty_ShouldThrow()
    {
        var theater = new Theater(1, [new SeatCode("A1")]);

        var act = () => theater.NormalizeAndValidateSeatCodes([]);

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void NormalizeAndValidateSeatCodes_WhenDuplicates_ShouldThrow()
    {
        var theater = new Theater(1, [new SeatCode("A1"), new SeatCode("A2")]);

        var act = () => theater.NormalizeAndValidateSeatCodes(["a1", " A1 "]);

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void NormalizeAndValidateSeatCodes_WhenSeatNotInTheater_ShouldThrow()
    {
        var theater = new Theater(1, [new SeatCode("A1")]);

        var act = () => theater.NormalizeAndValidateSeatCodes(["A2"]);

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void NormalizeAndValidateSeatCodes_WhenValid_ShouldReturnNormalizedSeatCodes()
    {
        var theater = new Theater(1, [new SeatCode("A1"), new SeatCode("B2")]);

        var normalized = theater.NormalizeAndValidateSeatCodes([" a1 ", "b2"]);

        normalized.Should().HaveCount(2);
        normalized.Select(x => x.Value).Should().BeEquivalentTo(["A1", "B2"]);
    }
}