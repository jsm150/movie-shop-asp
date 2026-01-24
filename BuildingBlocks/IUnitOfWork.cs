using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Domain;

public interface IUnitOfWork
{
    Task SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
