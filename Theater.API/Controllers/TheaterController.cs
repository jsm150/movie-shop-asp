using MediatR;
using Microsoft.AspNetCore.Mvc;
using Theater.API.Application.Commands;


namespace Theater.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TheaterController(IMediator mediator) : Controller
{
    [HttpPost]
    public async Task<ActionResult<long>> CreateTheater(
        CreateTheaterCommand command, 
        CancellationToken cancellationToken)
    {
        var theaterId = await mediator.Send(command, cancellationToken);
        return Ok(theaterId);
    }
}
