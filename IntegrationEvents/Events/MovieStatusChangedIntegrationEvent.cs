using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationEvents.Events;

public record MovieStatusChangedIntegrationEvent(long MovieId, MovieStatus MovieStatus) : INotification;


