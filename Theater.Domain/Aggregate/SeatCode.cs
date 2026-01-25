using Theater.Domain.Exceptions;

namespace Theater.Domain.Aggregate;

public readonly record struct SeatCode
{
    public string Value { get; }

    public SeatCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new TheaterDomainException("좌석 코드는 비어 있을 수 없습니다.");

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > 10)
            throw new TheaterDomainException("좌석 코드는 10자를 초과할 수 없습니다.");

        Value = normalized;
    }

    public override string ToString() => Value;
}