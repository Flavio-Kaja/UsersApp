namespace UserService.Domain.RolePermissions.Features;

using UserService.Services;
using UserService.Domain;
using HeimGuard;
using MediatR;
using UserService.Exceptions;
using UserService.Domain.Roles;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UserService.Domain.RolePermissions.Mappings;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.RolePermissions.Dtos;

public static class DeleteRolePermission
{
    public sealed class Command : IRequest
    {

        public readonly PostRolePermissionDto RolePermissionToAdd;
        public Command(PostRolePermissionDto rolePermissionForCreationDto)
        {
            RolePermissionToAdd = rolePermissionForCreationDto;
        }
    }

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<Handler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork, RoleManager<Role> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == request.RolePermissionToAdd.Role, cancellationToken: cancellationToken);
            if (role is null)
            {
                throw new NotFoundException($"Role with name {request.RolePermissionToAdd.Role} not found");
            }
            var claimResult = await _roleManager.RemoveClaimAsync(role, new System.Security.Claims.Claim(ClaimTypes.AuthorizationDecision, request.RolePermissionToAdd.Permission));
            if (!claimResult.Succeeded)
            {
                _logger.LogError("Removing role permissions failed: role object {@user}, identity result: {@identityResult}", role, claimResult);
                throw new IdentityException(string.Join(", ", claimResult.Errors));
            }
        }
    }
}