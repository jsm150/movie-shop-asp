using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationEvents.Events;

public record MovieDeletedIntegrationEvent : INotification
{
    public long MovieId { get; init; }
}
