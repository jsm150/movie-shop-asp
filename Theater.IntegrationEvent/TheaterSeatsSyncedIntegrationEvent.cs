using MediatR;

namespace Theater.IntegrationEvent;

// Theater 모듈에서 "해당 상영관 좌석맵이 현재 이렇다"를 스냅샷으로 발행(권장)
public record TheaterSeatsSyncedIntegrationEvent : INotification
{
    public long TheaterId { get; init; }
    public IReadOnlyCollection<string> SeatCodes { get; init; } = [];
}