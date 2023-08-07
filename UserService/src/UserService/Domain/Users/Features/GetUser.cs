namespace UserService.Domain.Users.Features;

using UserService.Domain.Users.Dtos;
using UserService.Domain.Users.Services;
using UserService.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using UserService.Exceptions;

public static class GetUser
{
    public sealed class Query : IRequest<UserDto>
    {
        public readonly Guid Id;

        public Query(Guid id)
        {
            Id = id;
        }
    }

    public sealed class Handler : IRequestHandler<Query, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IHeimGuardClient heimGuard)
        {
            _userRepository = userRepository;
            _heimGuard = heimGuard;
        }

        public async Task<UserDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUsers);

            var result = await _userRepository.GetById(request.Id, cancellationToken: cancellationToken);
            return result.ToUserDto();
        }
    }
}