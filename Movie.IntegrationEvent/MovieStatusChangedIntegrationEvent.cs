using MediatR;


namespace Movie.IntegrationEvent;

public record MovieStatusChangedIntegrationEvent(long MovieId, MovieStatus MovieStatus) : INotification;


