using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Kernel.Domain
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
