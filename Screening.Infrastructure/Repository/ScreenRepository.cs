using BuildingBlocks.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.ScreenAggregate;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Screening.Infrastructure.Repository;

public class ScreenRepository(IScreeningContext context) : IScreenRepository
{
    public IUnitOfWork UnitOfWork => context;

    public void Add(Screen screen)
    {
        context.Screens.Add(screen);
    }

    public async Task<Screen?> FindAsync(long screenId)
    {
        return await context.Screens.FindAsync(screenId);
    }

    public async Task<Screen?> FindByTheaterId(long theaterId)
    {
        return await context.Screens.FirstOrDefaultAsync(s => s.TheaterId == theaterId);
    }

    public async Task<bool> HasConflict(long theaterId, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        return await context.Screens.AnyAsync(
            s => s.TheaterId == theaterId &&
                 s.StartTime < endTime &&
                 startTime < s.EndTime);
    }

    public void Remove(Screen value)
    {
        context.Screens.Remove(value);
    }
}
