using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Domain;

public interface IUnitOfWork : IDisposable
{
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
