using MediatR;
using Movie.Domain.Aggregate;
using Movie.Domain.Exceptions;

namespace Movie.API.Application.Commands;

public class ChangeMovieStatusCommandHandler(IMovieRepository movieRepository) : IRequestHandler<ChangeMovieStatusCommand, bool>
{
    public async Task<bool> Handle(ChangeMovieStatusCommand request, CancellationToken cancellationToken)
    {
        var movie = await movieRepository.GetAsync(request.MovieId)
            ?? throw new MovieDomainException("대상 영화를 찾을 수 없습니다.");

        if (movie.MovieStatus == request.Status)
        {
            return true;
        }

        switch (request.Status)
        {
            case MovieStatus.COMMING_SOON:
                movie.MoveToCommingSoon();
                break;

            case MovieStatus.NOW_SHOWING:
                movie.StartShowing();
                break;

            case MovieStatus.ENDED:
                movie.EndShowing();
                break;

            case MovieStatus.PREPARING:
            default:
                throw new MovieDomainException("지원하지 않는 상태 변경입니다.");
        }

        return await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}