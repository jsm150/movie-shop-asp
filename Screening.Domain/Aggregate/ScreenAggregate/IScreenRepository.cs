using BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Screening.Domain.Aggregate.ScreenAggregate;

public interface IScreenRepository : IRepository<Screen>
{
    Task<Screen?> FindByTheaterIdAsync(long theaterId, CancellationToken cancellationToken);
}
