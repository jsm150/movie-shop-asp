using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Domain.Exceptions;

namespace Screening.Domain.Aggregate.ScreenAggregate;

public class SeatHold
{
    public long ScreenId { get; private set; }
    public SeatCode SeatCode { get; private set; }
    public Guid HoldToken { get; private set; }
    public SeatHoldStatus Status { get; private set; }
    public DateTimeOffset? HeldUntil { get; private set; }

    private SeatHold() { }

    internal SeatHold(long screenId, SeatCode seatCode, Guid holdToken, DateTimeOffset heldUntil)
    {
        ScreenId = screenId;
        SeatCode = seatCode;
        HoldToken = holdToken;
        Status = SeatHoldStatus.Held;
        HeldUntil = heldUntil;
    }

    internal bool IsActiveAt(DateTimeOffset now)
        => Status == SeatHoldStatus.Confirmed
           || (Status == SeatHoldStatus.Held && HeldUntil.HasValue && now < HeldUntil.Value);

    internal void Confirm(DateTimeOffset now)
    {
        if (Status != SeatHoldStatus.Held)
            throw new ScreeningDomainException("HELD 상태의 좌석만 확정할 수 있습니다.");

        if (!HeldUntil.HasValue || now >= HeldUntil.Value)
            throw new ScreeningDomainException("좌석 점유가 만료되어 확정할 수 없습니다.");

        Status = SeatHoldStatus.Confirmed;
        HeldUntil = null;
    }

    internal void Release()
    {
        if (Status == SeatHoldStatus.Released)
            return;

        if (Status == SeatHoldStatus.Confirmed)
            throw new ScreeningDomainException("확정된 좌석은 해제할 수 없습니다.");

        Status = SeatHoldStatus.Released;
        HeldUntil = null;
    }
}