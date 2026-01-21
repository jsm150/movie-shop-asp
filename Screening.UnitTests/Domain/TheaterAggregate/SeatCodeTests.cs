using FluentAssertions;
using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Domain.Exceptions;

namespace Screening.UnitTests.Domain.TheaterAggregate;

public class SeatCodeTests
{
    [Fact]
    public void Ctor_ShouldNormalize_Trim_AndUppercase()
    {
        var seatCode = new SeatCode("  a10 ");

        seatCode.Value.Should().Be("A10");
        seatCode.ToString().Should().Be("A10");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_WhenNullOrWhitespace_ShouldThrow(string? value)
    {
        var act = () => _ = new SeatCode(value!);

        act.Should().Throw<ScreeningDomainException>();
    }

    [Fact]
    public void Ctor_WhenLengthGreaterThan10_ShouldThrow()
    {
        var act = () => _ = new SeatCode("ABCDEFGHIJK"); // 11

        act.Should().Throw<ScreeningDomainException>();
    }
}