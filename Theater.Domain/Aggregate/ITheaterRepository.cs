using BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Theater.Domain.Aggregate;

public interface ITheaterRepository : IRepository<Theater, long>
{
}
