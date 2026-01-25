using BuildingBlocks.API.Application;

namespace Screening.API.Application.Commands;

public sealed record DeleteScreenCommand(long ScreenId) : ICommand<bool>;