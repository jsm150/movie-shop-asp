namespace Screening.Domain.Aggregate.TheaterAggregate;

public class TheaterSeat
{
    public long TheaterId { get; private set; }
    public SeatCode SeatCode { get; private set; }

    private TheaterSeat() { }

    internal TheaterSeat(long theaterId, SeatCode seatCode)
    {
        if (theaterId <= 0) throw new ArgumentOutOfRangeException(nameof(theaterId));

        TheaterId = theaterId;
        SeatCode = seatCode;
    }
}