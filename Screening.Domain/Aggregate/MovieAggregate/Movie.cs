using BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Screening.Domain.Aggregate.MovieAggregate
{
    public class Movie : IAggregateRoot
    {
        public long MovieId { get; init; }
        public MovieStatus MovieStatus { get; set; }

        public bool CanBeScreened()
        {
            return MovieStatus == MovieStatus.NOW_SHOWING;
        }
    }
}
