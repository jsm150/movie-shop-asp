using Movie.Domain.Exceptions;
using SeedWork.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.Domain.Aggregate;

public class Movie : IAggregateRoot
{
    public long MovieId { get; private set; }
    public required MovieInfo MovieInfo { get; init; } 
    public MovieStatus MovieStatus { get; private set; } = MovieStatus.PREPARING;

    public void MoveToCommingSoon()
    {
        if (MovieStatus == MovieStatus.PREPARING)
        {
            MovieStatus = MovieStatus.COMMING_SOON;
        }
        else
        {
            throw new MovieDomainException("PREPARING 이 아닌 상태에서 COMMING_SOON으로 변경하려고 함.");
        }

    }

    public void StartShowing()
    {
        if (MovieStatus == MovieStatus.COMMING_SOON)
        {
            MovieStatus = MovieStatus.NOW_SHOWING;
        }
        else
        {
            throw new MovieDomainException("COMMING_SOON 이 아닌 상태에서 NOW_SHOWING으로 변경하려고 함.");
        }
    }

    public void EndShowing()
    {
        if (MovieStatus == MovieStatus.NOW_SHOWING)
        {
            MovieStatus = MovieStatus.ENDED;
        }
        else
        {
            throw new MovieDomainException("NOW_SHOWING 이 아닌 상태에서 ENDED로 변경하려고 함.");
        }
    }
}
