using BuildingBlocks.Domain;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Screening.Domain.Exceptions;

namespace Screening.Domain.Aggregate.ScreenAggregate
{
    public class Screen : IAggregateRoot
    {
        private readonly List<SeatHold> _seatHolds = [];

        public long ScreenId { get; private set; }
        public long MovieId { get; private set; }
        public long TheaterId { get; private set; }

        public DateTimeOffset StartTime { get; private set; }
        public DateTimeOffset EndTime { get; private set; }

        public DateTimeOffset SalesStartAt { get; private set; }
        public DateTimeOffset SalesEndAt { get; private set; }

        public ScreenStatus Status { get; private set; } = ScreenStatus.SCHEDULED;

        public IReadOnlyCollection<SeatHold> SeatHolds => _seatHolds;

        private Screen() { }

        public static async Task<Screen> CreateAsync(
            Movie movie,
            long theaterId,
            IScreenRepository repository,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            DateTimeOffset salesStartAt,
            DateTimeOffset salesEndAt)
        {
            if (endTime <= startTime)
                throw new ScreeningDomainException("EndTime은 StartTime 이후여야 합니다.");
            if (salesEndAt <= salesStartAt)
                throw new ScreeningDomainException("SalesEndAt은 SalesStartAt 이후여야 합니다.");
            if (!movie.CanBeScreened())
                throw new ScreeningDomainException("상영 가능한 상태의 영화가 아닙니다.");
            if (await repository.HasConflict(theaterId, startTime, endTime))
                throw new ScreeningDomainException("요청 시간에 해당 상영관의 상영이 이미 예약되어 있습니다.");

            return new Screen
            {
                MovieId = movie.MovieId,
                TheaterId = theaterId,
                StartTime = startTime,
                EndTime = endTime,
                SalesStartAt = salesStartAt,
                SalesEndAt = salesEndAt
            };
        }

        public async Task UpdateAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            IScreenRepository repository,
            DateTimeOffset salesStartAt,
            DateTimeOffset salesEndAt)
        {
            if (Status != ScreenStatus.SCHEDULED)
                throw new ScreeningDomainException("예정 상태의 상영만 수정할 수 있습니다.");

            if (endTime <= startTime)
                throw new ScreeningDomainException("EndTime은 StartTime 이후여야 합니다.");

            if (salesEndAt <= salesStartAt)
                throw new ScreeningDomainException("SalesEndAt은 SalesStartAt 이후여야 합니다.");

            if (await repository.HasConflict(TheaterId, startTime, endTime))
                throw new ScreeningDomainException("요청 시간에 해당 상영관의 상영이 이미 예약되어 있습니다.");

            StartTime = startTime;
            EndTime = endTime;
            SalesStartAt = salesStartAt;
            SalesEndAt = salesEndAt;
        }

        public void RemoveValidate()
        {
            if (Status != ScreenStatus.SCHEDULED)
                throw new ScreeningDomainException("예정 상태의 상영만 취소할 수 있습니다.");
        }

        public bool IsPublished() => Status != ScreenStatus.SCHEDULED;

        public void HoldSeats(Theater theater, IReadOnlyCollection<string> seatCodes, Guid holdToken, DateTimeOffset heldUntil, DateTimeOffset now)
        {
            ArgumentNullException.ThrowIfNull(theater);

            EnsureSaleable(now);

            if (theater.TheaterId != TheaterId)
                throw new ScreeningDomainException("상영관 정보가 일치하지 않습니다.");

            if (heldUntil <= now)
                throw new ScreeningDomainException("HeldUntil은 현재 시간 이후여야 합니다.");

            var normalized = theater.NormalizeAndValidateSeatCodes(seatCodes);

            foreach (var seatCode in normalized)
            {
                var alreadyActive = _seatHolds.Any(h => h.SeatCode == seatCode && h.IsActiveAt(now));
                if (alreadyActive)
                    throw new ScreeningDomainException($"이미 점유/예약된 좌석입니다. seatCode={seatCode}");

                _seatHolds.Add(new SeatHold(ScreenId, seatCode, holdToken, heldUntil));
            }
        }

        public void ConfirmSeats(Guid holdToken, DateTimeOffset now)
        {
            var holds = _seatHolds.Where(h => h.HoldToken == holdToken).ToList();
            if (holds.Count == 0)
                throw new ScreeningDomainException("확정할 점유가 없습니다.");

            foreach (var hold in holds)
                hold.Confirm(now);
        }

        public void ReleaseSeats(Guid holdToken)
        {
            var holds = _seatHolds.Where(h => h.HoldToken == holdToken).ToList();

            foreach (var hold in holds)
                hold.Release();
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
