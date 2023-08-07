using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Data.Entity;
using System.Security.Claims;
using UserService.Domain.RolePermissions.Dtos;
using UserService.Domain.Roles.Dtos;
using UserService.Domain.Roles.Mappings;
using UserService.Exceptions;

namespace UserService.Domain.Roles.Features;
public static class GetRolesList
{
    public sealed class Query : IRequest<IList<RoleDto>>
    {

        public Query()
        {
        }
    }

    public sealed class Handler : IRequestHandler<Query, IList<RoleDto>>
    {
        private readonly ILogger<Handler> _logger;
        private readonly RoleManager<Role> _roleManager;

        public Handler(ILogger<Handler> logger, RoleManager<Role> roleManager)
        {
            _logger = logger;
            _roleManager = roleManager;
        }

        public async Task<IList<RoleDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving roles list");
            var roles = await _roleManager.Roles.ToRoleDtoQueryable().ToListAsync();
            return roles ?? new List<RoleDto>();
        }
    }
}
