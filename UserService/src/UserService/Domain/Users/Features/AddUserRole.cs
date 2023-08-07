namespace UserService.Domain.Users.Features;

using UserService.Domain.Users.Services;
using UserService.Domain.Users;
using UserService.Domain.Users.Dtos;
using UserService.Services;
using HeimGuard;
using Mappings;
using MediatR;
using Roles;
using UserService.Exceptions;
using Microsoft.AspNetCore.Identity;

public static class AddUserRole
{
    public sealed class Command : IRequest
    {
        public readonly Guid UserId;
        public readonly string Role;
        public readonly bool SkipPermissions;

        public Command(Guid userId, string role, bool skipPermissions = false)
        {
            UserId = userId;
            Role = role;
            SkipPermissions = skipPermissions;
        }
    }

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard, UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
            _userManager = userManager;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            if (!request.SkipPermissions)
                await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddUserRoles);

            var user = await _userRepository.GetById(request.UserId, true, cancellationToken);
            await _userManager.AddToRoleAsync(user, request.Role);
            var roleToAdd = user.AddRole(new Role(request.Role));

            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}