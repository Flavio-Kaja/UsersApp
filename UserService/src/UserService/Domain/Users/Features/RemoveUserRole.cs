namespace UserService.Domain.Users.Features;

using UserService.Domain.Users.Services;
using UserService.Domain.Users;
using UserService.Domain.Users.Dtos;
using UserService.Services;
using HeimGuard;
using MediatR;
using Roles;
using UserService.Exceptions;

public static class RemoveUserRole
{
    public sealed class Command : IRequest
    {
        public readonly Guid UserId;
        public readonly string Role;

        public Command(Guid userId, string role)
        {
            UserId = userId;
            Role = role;
        }
    }

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanRemoveUserRoles);
            var user = await _userRepository.GetById(request.UserId, true, cancellationToken);

            var roleToRemove = user.RemoveRole(new Role(request.Role));
            _userRepository.RemoveRole(roleToRemove);
            _userRepository.Update(user);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}