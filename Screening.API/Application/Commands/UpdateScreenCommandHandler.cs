using MediatR;
using Microsoft.EntityFrameworkCore;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Exceptions;

namespace Screening.API.Application.Commands;

public class UpdateScreenCommandHandler(IScreenRepository screenRepository)
    : IRequestHandler<UpdateScreenCommand, bool>
{
    public async Task<bool> Handle(UpdateScreenCommand request, CancellationToken cancellationToken)
    {
        var screen = await screenRepository.FindAsync(request.ScreenId)
            ?? throw new ScreeningDomainException("해당 상영 시간표를 찾을 수 없습니다.");

        await screen.UpdateAsync(
            request.StartTime,
            request.EndTime,
            screenRepository,
            request.SalesStartAt,
            request.SalesEndAt);

        await screenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return true;
    }
}