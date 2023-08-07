
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Authentication.Features;
using UserService.Authentication.Models;

namespace UserService.Controllers.v1
{
    [ApiController]
    [Route("api/authentication")]
    [ApiVersion("1.0")]
    public class AuthenticationController : ControllerBase
    {

        private readonly IMediator _mediator;
        private readonly IValidator<UserLoginModel> _validator;
        public AuthenticationController(IMediator mediator, IValidator<UserLoginModel> validator)
        {
            _mediator = mediator;
            _validator = validator;
        }

        [HttpPost("login", Name = "Login")]
        public async Task<IActionResult> UserLoginAsync([FromBody] UserLoginModel request)
        {
            _validator.ValidateAndThrow(request);
            var command = new UserLoginCommand(request);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

    }
}
