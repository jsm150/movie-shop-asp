using MediatR;
using Microsoft.AspNetCore.Mvc;
using Screening.API.Application.Commands;

namespace Screening.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreenController(IMediator mediator) : Controller
{
    [HttpPost]
    public async Task<ActionResult<long>> Create(CreateScreenCommand command, CancellationToken cancellationToken)
    {
        var screenId = await mediator.Send(command, cancellationToken);
        return Ok(screenId);
    }
}
