using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.ScreenAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Screening.Infrastructure.Repositories;

public class ScreenRepository(IScreeningContext context) : IScreenRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Screen?> FindByTheaterIdAsync(long theaterId, CancellationToken cancellationToken)
    {
        return await context.Screens
            .FirstOrDefaultAsync(s => s.TheaterId == theaterId, cancellationToken);
    }
}
