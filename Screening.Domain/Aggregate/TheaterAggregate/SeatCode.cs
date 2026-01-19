using Screening.Domain.Exceptions;

namespace Screening.Domain.Aggregate.TheaterAggregate;

public readonly record struct SeatCode
{
    public string Value { get; }

    public SeatCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ScreeningDomainException("좌석 코드는 비어 있을 수 없습니다.");

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > 10)
            throw new ScreeningDomainException("좌석 코드는 10자를 초과할 수 없습니다.");

        Value = normalized;
    }

    public override string ToString() => Value;
}