using BuildingBlocks.Domain;
using Screening.Domain.Exceptions;


namespace Screening.Domain.Aggregate.ScreenAggregate
{
    public class Screen : IAggregateRoot
    {
        private readonly HashSet<string> _reservedSeatCodes = [];

        public long ScreenId { get; private set; }
        public long MovieId { get; private set; }
        public long TheaterId { get; init; }

        public DateTimeOffset StartTime { get; private set; }
        public DateTimeOffset EndTime { get; private set; }

        public DateTimeOffset SalesStartAt { get; private set; }
        public DateTimeOffset SalesEndAt { get; private set; }

        public ScreenStatus Status { get; private set; } = ScreenStatus.SCHEDULED;

        public IReadOnlyCollection<string> ReservedSeatCodes => _reservedSeatCodes;

        private Screen() { }

        public Screen(
            MovieAggregate.Movie movie,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            DateTimeOffset salesStartAt,
            DateTimeOffset salesEndAt)
        {
            if (endTime <= startTime)
            {
                throw new ScreeningDomainException("EndTime은 StartTime 이후여야 합니다.");
            }
            if (salesEndAt <= salesStartAt)
            {
                throw new ScreeningDomainException("SalesEndAt은 SalesStartAt 이후여야 합니다.");
            }
            if (!movie.CanBeScreened())
            {
                throw new ScreeningDomainException("상영 가능한 상태의 영화가 아닙니다.");
            }


            MovieId = movie.MovieId;
            StartTime = startTime;
            EndTime = endTime;
            SalesStartAt = salesStartAt;
            SalesEndAt = salesEndAt;
        }

        public void OpenSales()
        {
            if (Status != ScreenStatus.SCHEDULED)
                throw new ScreeningDomainException("SCHEDULED 상태에서만 예매 오픈이 가능합니다.");

            Status = ScreenStatus.ON_SALE;
        }

        public void CloseSales()
        {
            if (Status != ScreenStatus.ON_SALE)
                throw new ScreeningDomainException("ON_SALE 상태에서만 예매 마감이 가능합니다.");

            Status = ScreenStatus.SALES_CLOSED;
        }

        public void Cancel(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ScreeningDomainException("취소 사유는 필수입니다.");
            if (Status == ScreenStatus.ENDED)
                throw new ScreeningDomainException("상영 종료된 건은 취소할 수 없습니다.");

            Status = ScreenStatus.CANCELED;
        }

        public void End()
        {
            if (Status == ScreenStatus.CANCELED)
                throw new ScreeningDomainException("취소된 상영은 종료 처리할 수 없습니다.");

            Status = ScreenStatus.ENDED;
        }

        public void ReserveSeats(IReadOnlyCollection<string> seatCodes, DateTimeOffset now)
        {
            EnsureSaleable(now);

            if (seatCodes == null || seatCodes.Count == 0)
                throw new ScreeningDomainException("예약할 좌석이 없습니다.");

            foreach (var seatCode in seatCodes)
            {
                if (string.IsNullOrWhiteSpace(seatCode))
                    throw new ScreeningDomainException("좌석 코드는 비어 있을 수 없습니다.");

                if (!_reservedSeatCodes.Add(seatCode))
                    throw new ScreeningDomainException($"이미 예약된 좌석입니다. seatCode={seatCode}");
            }
        }

        public void ReleaseSeats(IReadOnlyCollection<string> seatCodes)
        {
            if (seatCodes == null || seatCodes.Count == 0)
                throw new ScreeningDomainException("해제할 좌석이 없습니다.");

            foreach (var seatCode in seatCodes)
            {
                if (string.IsNullOrWhiteSpace(seatCode))
                    throw new ScreeningDomainException("좌석 코드는 비어 있을 수 없습니다.");

                if (!_reservedSeatCodes.Remove(seatCode))
                    throw new ScreeningDomainException($"예약되지 않은 좌석은 해제할 수 없습니다. seatCode={seatCode}");
            }
        }

        private void EnsureSaleable(DateTimeOffset now)
        {
            if (Status != ScreenStatus.ON_SALE)
                throw new ScreeningDomainException("예매 가능한 상태가 아닙니다.");

            if (now < SalesStartAt || now > SalesEndAt)
                throw new ScreeningDomainException("예매 가능 시간 범위가 아닙니다.");

            if (now >= StartTime)
                throw new ScreeningDomainException("상영 시작 이후에는 예매할 수 없습니다.");
        }
    }
}
