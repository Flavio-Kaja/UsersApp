namespace UserService.Domain.Users.Features;

using UserService.Domain.Users;
using UserService.Domain.Users.Dtos;
using UserService.Domain.Users.Services;
using UserService.Services;
using UserService.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using UserService.Exceptions;
using Microsoft.AspNetCore.Identity;

public static class UpdateUser
{
    public sealed class Command : IRequest
    {
        public readonly Guid Id;
        public readonly PostUserDto UpdatedUserData;

        public Command(Guid id, PostUserDto updatedUserData)
        {
            Id = id;
            UpdatedUserData = updatedUserData;
        }
    }

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<Handler> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard, UserManager<User> userManager, ILogger<Handler> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var userToUpdate = await _userRepository.GetById(request.Id, cancellationToken: cancellationToken);
            userToUpdate.Update(request.UpdatedUserData);
            var identityResult = await _userManager.UpdateAsync(userToUpdate);
            if (!identityResult.Succeeded)
            {
                _logger.LogError("User update failed : user object {@user}, identity result: {@identityResult}", request.UpdatedUserData, identityResult);
                throw new IdentityException(string.Join(", ", identityResult.Errors));
            }
        }
    }
}