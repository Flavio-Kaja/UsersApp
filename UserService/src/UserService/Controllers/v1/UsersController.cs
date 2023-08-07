namespace UserService.Controllers.v1;

using UserService.Domain.Users.Features;
using UserService.Domain.Users.Dtos;
using UserService.Wrappers;
using UserService.Domain;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using MediatR;
using FluentValidation;

[ApiController]
[Route("api/users")]
[ApiVersion("1.0")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<PostUserDto> _validator;

    public UsersController(IMediator mediator, IValidator<PostUserDto> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    /// <summary>
    /// Adds a new role to a user.
    /// </summary>
    [Authorize]
    [HttpPut("{userId:guid}/addRole", Name = "AddRole")]
    public async Task<IActionResult> AddRole([FromRoute] Guid userId, [FromBody] string role)
    {
        var command = new AddUserRole.Command(userId, role);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Removes a role from a User
    /// </summary>
    [Authorize]
    [HttpPut("{userId:guid}/removeRole", Name = "RemoveRole")]
    public async Task<ActionResult> RemoveRole([FromRoute] Guid userId, [FromBody] string role)
    {
        var command = new RemoveUserRole.Command(userId, role);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Gets a list of all Users.
    /// </summary>
    [Authorize(Policy = Permissions.CanReadUsers)]
    [HttpGet(Name = "GetUsers")]
    public async Task<IActionResult> GetUsers([FromQuery] UserParametersDto userParametersDto)
    {
        var query = new GetUserList.Query(userParametersDto);
        var queryResponse = await _mediator.Send(query);

        var paginationMetadata = new
        {
            totalCount = queryResponse.TotalCount,
            pageSize = queryResponse.PageSize,
            currentPageSize = queryResponse.CurrentPageSize,
            currentStartIndex = queryResponse.CurrentStartIndex,
            currentEndIndex = queryResponse.CurrentEndIndex,
            pageNumber = queryResponse.PageNumber,
            totalPages = queryResponse.TotalPages,
            hasPrevious = queryResponse.HasPrevious,
            hasNext = queryResponse.HasNext
        };

        Response.Headers.Add("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single User by ID.
    /// </summary>
    [Authorize(Policy = Permissions.CanReadUsers)]
    [HttpGet("{id:guid}", Name = "GetUser")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var query = new GetUser.Query(id);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new User record.
    /// </summary>
    [Authorize(Policy = Permissions.CanAddUsers)]
    [HttpPost(Name = "AddUser")]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] PostUserDto userForCreation)
    {
        _validator.ValidateAndThrow(userForCreation);
        var command = new AddUser.Command(userForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetUser",
            new { commandResponse.Id },
            commandResponse);
    }

    /// <summary>
    /// Updates an entire existing User.
    /// </summary>
    [Authorize(Policy = Permissions.CanUpdateUsers)]
    [HttpPut("{id:guid}", Name = "UpdateUser")]
    public async Task<IActionResult> UpdateUser(Guid id, PostUserDto user)
    {
        _validator.ValidateAndThrow(user);
        var command = new UpdateUser.Command(id, user);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing User record.
    /// </summary>
    [Authorize(Policy = Permissions.CanDeleteUsers)]
    [HttpDelete("{id:guid}", Name = "DeleteUser")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUser.Command(id);
        await _mediator.Send(command);
        return NoContent();
    }

}
