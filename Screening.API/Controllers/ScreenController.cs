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

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> Update(long id, UpdateScreenCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ScreenId)
            return BadRequest("경로의 ID와 요청 본문의 ScreenId가 일치하지 않습니다.");

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(long id, CancellationToken cancellationToken)
    {
        var command = new DeleteScreenCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
