namespace UserService.Controllers.v1;

using Domain;
using HeimGuard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Exceptions;
using UserService.Domain.Roles.Features;

[ApiController]
[Route("api/roles")]
[ApiVersion("1.0")]
public sealed class RolesController : ControllerBase
{
    private readonly IHeimGuardClient _heimGuard;
    private readonly IMediator _mediator;
    public RolesController(IHeimGuardClient heimGuard, IMediator mediator)
    {
        _heimGuard = heimGuard;
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a list of all available roles.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetRoles")]
    public async Task<IActionResult> GetRoles()
    {
        var command = new GetRolesList.Query();
        return Ok(await _mediator.Send(command));
    }
}