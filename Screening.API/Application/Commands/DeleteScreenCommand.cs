using MediatR;

namespace Screening.API.Application.Commands;

public sealed record DeleteScreenCommand(long ScreenId) : IRequest<bool>;