using Theater.Domain.Exceptions;

namespace Theater.Domain.Aggregate;

public class TheaterSeat
{
    public long TheaterId { get; init; }
    public string SeatCode { get; private set; } = null!;

    private TheaterSeat() { }

    // Theater Aggregate 내부에서 생성할 때 사용
    internal TheaterSeat(string seatCode)
    {
        if (string.IsNullOrWhiteSpace(seatCode))
            throw new TheaterDomainException("좌석 코드는 비어 있을 수 없습니다.");

        var normalized = seatCode.Trim().ToUpperInvariant();

        if (normalized.Length > 10)
            throw new TheaterDomainException("좌석 코드는 10자를 초과할 수 없습니다.");

        SeatCode = normalized;
    }
}