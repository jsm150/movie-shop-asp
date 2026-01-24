using BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Screening.Domain.Aggregate.MovieAggregate;

public interface IScreeningMovieRepository : IRepository<Movie, long>
{
    
}
