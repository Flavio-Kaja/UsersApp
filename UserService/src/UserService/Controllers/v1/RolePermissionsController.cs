namespace UserService.Controllers.v1;

using UserService.Domain.RolePermissions.Features;
using UserService.Domain.RolePermissions.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;

[ApiController]
[Route("api/rolepermissions")]
[ApiVersion("1.0")]
public sealed class RolePermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<PostRolePermissionDto> _validator;
    public RolePermissionsController(IMediator mediator, IValidator<PostRolePermissionDto> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }


    /// <summary>
    /// Gets a list of all claims for a role.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetRolePermissions")]
    public async Task<IActionResult> GetRolePermissions([FromQuery] RolePermissionParametersDto rolePermissionParametersDto)
    {
        var query = new GetRolePermissionList.Query(rolePermissionParametersDto);
        var queryResponse = await _mediator.Send(query);

        return Ok(queryResponse);
    }

    /// <summary>
    /// Add a new claim to a role.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddRolePermission")]
    public async Task<ActionResult<RolePermissionDto>> AddRolePermission([FromBody] PostRolePermissionDto rolePermissionForCreation)
    {
        _validator.ValidateAndThrow(rolePermissionForCreation);
        var command = new AddRolePermission.Command(rolePermissionForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetRolePermission",
            new { commandResponse.Id },
            commandResponse);
    }

    /// <summary>
    /// Deletes an existing claim from a role.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:guid}", Name = "DeleteRolePermission")]
    public async Task<ActionResult> DeleteRolePermission([FromBody] PostRolePermissionDto rolePermission)
    {
        _validator.ValidateAndThrow(rolePermission);
        var command = new DeleteRolePermission.Command(rolePermission);
        await _mediator.Send(command);
        return NoContent();
    }
}
