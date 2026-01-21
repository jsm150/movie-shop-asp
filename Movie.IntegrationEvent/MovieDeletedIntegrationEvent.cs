using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.IntegrationEvent;

public record MovieDeletedIntegrationEvent : INotification
{
    public long MovieId { get; init; }
}
