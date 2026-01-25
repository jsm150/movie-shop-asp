using BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Theater.Domain.Aggregate;

namespace Theater.Infrastructure.Repositories;

public class TheaterRepository(ITheaterContext context) : ITheaterRepository
{
    public IUnitOfWork UnitOfWork => context;

    public void Add(Domain.Aggregate.Theater value)
    {
        context.Theaters.Add(value);
    }

    public async Task<Domain.Aggregate.Theater?> FindAsync(long id)
    {
        return await context.Theaters.FindAsync(id);
    }

    public void Remove(Domain.Aggregate.Theater value)
    {
        context.Theaters.Remove(value);
    }
}
