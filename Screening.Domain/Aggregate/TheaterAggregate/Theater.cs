using BuildingBlocks.Domain;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Exceptions;
using System.Collections.Generic;

namespace Screening.Domain.Aggregate.TheaterAggregate;

public class Theater
{
    private readonly List<TheaterSeat> _seats = [];

    public long TheaterId { get; private set; }

    public IReadOnlyCollection<TheaterSeat> Seats => _seats;

    private Theater() { }

    public Theater(long theaterId, IReadOnlyCollection<SeatCode> seatCodes)
    {
        ArgumentNullException.ThrowIfNull(seatCodes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(theaterId);
        TheaterId = theaterId;
        _seats.AddRange(seatCodes.Distinct().Select(seatCode => new TheaterSeat(theaterId, seatCode)));
    }

    public void ReplaceSeats(IReadOnlyCollection<SeatCode> seatCodes, Screen? screen)
    {
        ArgumentNullException.ThrowIfNull(seatCodes);

        if (screen != null)
        {
            if (screen.TheaterId != TheaterId)
                throw new ScreeningDomainException("상영관이 소속된 상영관과 일치하지 않습니다.");

            if (screen.IsPublished())
                throw new ScreeningDomainException("상영관이 확정된 이후에는 좌석 구성을 변경할 수 없습니다.");
        }

        _seats.Clear();
        _seats.AddRange(seatCodes.Distinct().Select(seatCode => new TheaterSeat(TheaterId, seatCode)));
    }

    public IReadOnlyCollection<SeatCode> NormalizeAndValidateSeatCodes(IReadOnlyCollection<string> seatCodes)
    {
        if (seatCodes == null || seatCodes.Count == 0)
            throw new ScreeningDomainException("좌석 코드가 없습니다.");

        var normalized = seatCodes.Select(x => new SeatCode(x)).ToArray();

        if (normalized.Distinct().Count() != normalized.Length)
            throw new ScreeningDomainException("중복된 좌석 코드가 포함되어 있습니다.");

        if (!ContainsAllSeats(normalized))
            throw new ScreeningDomainException("상영관에 존재하지 않는 좌석 코드가 포함되어 있습니다.");

        return normalized;
    }

    private bool ContainsAllSeats(IReadOnlyCollection<SeatCode> seatCodes)
    {
        var set = _seats.Select(x => x.SeatCode).ToHashSet();
        return seatCodes.All(set.Contains);
    }
}