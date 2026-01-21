using MediatR;
namespace Movie.IntegrationEvent;

public record MovieCreatedIntegrationEvent : INotification
{
    public long MovieId { get; init; }
    public MovieStatus MovieStatus { get; init; }
}

public enum MovieStatus
{
    PREPARING,
    COMMING_SOON,
    NOW_SHOWING,
    ENDED
}

