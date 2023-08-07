namespace UserService.Domain.RolePermissions.Features;

using UserService.Domain.RolePermissions.Dtos;
using UserService.Wrappers;
using UserService.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using UserService.Exceptions;
using UserService.Domain.Roles;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public static class GetRolePermissionList
{
    public sealed class Query : IRequest<IList<Claim>>
    {
        public readonly RolePermissionParametersDto QueryParameters;

        public Query(RolePermissionParametersDto queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }

    public sealed class Handler : IRequestHandler<Query, IList<Claim>>
    {
        private readonly ILogger<Handler> _logger;
        private readonly RoleManager<Role> _roleManager;

        public Handler(ILogger<Handler> logger, RoleManager<Role> roleManager)
        {
            _logger = logger;
            _roleManager = roleManager;
        }

        public async Task<IList<Claim>> Handle(Query request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == request.QueryParameters.Role, cancellationToken: cancellationToken);
            if (role is null)
            {
                _logger.LogError($"Role with name {request.QueryParameters.Role} not found");
                throw new NotFoundException($"Role with name {request.QueryParameters.Role} not found");
            }

            _logger.LogInformation("Retrieved role {@role}", role);
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims ?? new List<Claim>();
        }
    }
}