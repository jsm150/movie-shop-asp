using System;
using System.Collections.Generic;
using System.Text;

namespace SeedWork.Domain
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
