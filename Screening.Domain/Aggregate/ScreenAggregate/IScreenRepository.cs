using BuildingBlocks.Domain;
using MediatR;
using System.Threading;


namespace Screening.Domain.Aggregate.ScreenAggregate;

public interface IScreenRepository : IRepository<Screen, long>
{
    Task<Screen?> FindByTheaterId(long theaterId);

    Task<bool> HasConflict(long theaterId, DateTimeOffset startTime, DateTimeOffset endTime);
    Task<bool> Has(long theaterId);
}
