using MediatR;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Exceptions;

namespace Screening.API.Application.Commands;

public class DeleteScreenCommandHandler(IScreenRepository screenRepository)
    : IRequestHandler<DeleteScreenCommand, bool>
{
    public async Task<bool> Handle(DeleteScreenCommand request, CancellationToken cancellationToken)
    {
        var screen = await screenRepository.FindAsync(request.ScreenId)
            ?? throw new ScreeningDomainException("해당 상영 시간표를 찾을 수 없습니다.");

        screen.RemoveValidate();

        screenRepository.Remove(screen);
        await screenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return true;
    }
}