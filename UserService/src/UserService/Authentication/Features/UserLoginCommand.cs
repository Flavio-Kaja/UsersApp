using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using UserService.Authentication.Models;
using UserService.Authentication.Services;
using UserService.Domain.Roles;
using UserService.Domain.Users;
using UserService.Domain.Users.Services;
using UserService.Exceptions;

namespace UserService.Authentication.Features
{
    /// <summary>
    /// User login command.
    /// </summary>
    public class UserLoginCommand : IRequest<LoginResponseModel>
    {
        public readonly UserLoginModel userLoginModel;
        public UserLoginCommand(UserLoginModel userLoginModel)
        {
            this.userLoginModel = userLoginModel;
        }
    }

    /// <summary>
    /// Command handler
    /// </summary>
    public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, LoginResponseModel>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<UserLoginCommandHandler> _logger;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRepository _userRepository;

        public UserLoginCommandHandler(IAuthenticationService authenticationService,
            ILogger<UserLoginCommandHandler> logger, IUserRepository userRepository, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<LoginResponseModel> Handle(UserLoginCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.Query().Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Email == command.userLoginModel.Email, cancellationToken: cancellationToken);
                if (user is null)
                    throw new NotFoundException();

                var passwordCheckResult = await _userManager.CheckPasswordAsync(user, command.userLoginModel.Password);

                if (!passwordCheckResult)
                    throw new NotFoundException("Invalid username or password.");

                var token = await _authenticationService.AuthenticateAsync(user, cancellationToken);
                return new LoginResponseModel(token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while logging in.");
                throw;
            }
        }

    }
}