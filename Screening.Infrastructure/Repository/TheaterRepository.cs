using BuildingBlocks.Domain;
using Screening.Domain.Aggregate.TheaterAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Screening.Infrastructure.Repository;

public class TheaterRepository(IScreeningContext context) : ITheaterRepository
{
    public IUnitOfWork UnitOfWork => context;

    public void Add(Theater value)
    {
        context.Theaters.Add(value);
    }

    public async Task<Theater?> FindAsync(long id)
    {
        return await context.Theaters.FindAsync(id);
    }

    public void Remove(Theater value)
    {
        context.Theaters.Remove(value);
    }
}
