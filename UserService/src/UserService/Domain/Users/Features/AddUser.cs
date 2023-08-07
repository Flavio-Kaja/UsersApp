namespace UserService.Domain.Users.Features;

using UserService.Domain.Users.Services;
using UserService.Domain.Users;
using UserService.Domain.Users.Dtos;
using UserService.Services;
using UserService.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using UserService.Exceptions;
using Microsoft.AspNetCore.Identity;

public static class AddUser
{
    public sealed class Command : IRequest<UserDto>
    {
        public readonly PostUserDto UserToAdd;
        public readonly bool SkipPermissions;

        public Command(PostUserDto userToAdd, bool skipPermissions = false)
        {
            UserToAdd = userToAdd;
            SkipPermissions = skipPermissions;
        }
    }

    public sealed class Handler : IRequestHandler<Command, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;

        public Handler(IUserRepository userRepository, UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if (_userRepository.Query().Any(r => r.UserName == request.UserToAdd.Username))
                throw new ValidationException("Username", $"A user with username {request.UserToAdd.Username} already exists");
            var user = User.Create(request.UserToAdd);
            var result = await _userManager.CreateAsync(user, request.UserToAdd.Password);

            if (result.Succeeded)
            {

                await _userManager.AddToRoleAsync(user, "User");
                var userAdded = await _userRepository.GetById(user.Id, cancellationToken: cancellationToken);
                return userAdded.ToUserDto();
            }
            else
                throw new Exception("User Creation Failed");
        }
    }
}
