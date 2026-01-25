using BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using TheaterEntity = Theater.Domain.Aggregate.Theater;

namespace Theater.Infrastructure;

public interface ITheaterContext : IUnitOfWork
{
    DbSet<TheaterEntity> Theaters { get; }
}
