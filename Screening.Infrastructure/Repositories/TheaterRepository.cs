using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.TheaterAggregate;

namespace Screening.Infrastructure.Repositories;

public class TheaterRepository(IScreeningContext context) : ITheaterRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Theater?> GetAsync(long theaterId, CancellationToken cancellationToken)
    {
        return await context.Theaters
            .FirstOrDefaultAsync(x => x.TheaterId == theaterId, cancellationToken);
    }

    public void Add(Theater theater)
    {
        context.Theaters.Add(theater);
    }
    
}