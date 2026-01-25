namespace Theater.Domain.Aggregate;

public class TheaterSeat
{
    public long TheaterId { get; private set; }
    public SeatCode SeatCode { get; private set; }

    private TheaterSeat() { }

    internal TheaterSeat(long theaterId, SeatCode seatCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(theaterId);

        TheaterId = theaterId;
        SeatCode = seatCode;
    }
}