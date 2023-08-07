namespace UserService.Services;

using Domain.Roles;
using Domain.Users.Dtos;
using Domain.Users.Features;
using Domain.Users.Services;
using HeimGuard;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Exceptions;
using UserService.Domain.Users;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public sealed class UserPolicyHandler : IUserPolicyHandler
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;

    public UserPolicyHandler(ICurrentUserService currentUserService, IUserRepository userRepository, IMediator mediator, RoleManager<Role> roleManager, UserManager<User> userManager)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _mediator = mediator;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IEnumerable<string>> GetUserPermissions()
    {
        List<Claim> claims = new();
        var roles = await GetRoles();
        var currentUser = await _userRepository.Query().AsNoTracking().FirstOrDefaultAsync(u => u.Email == _currentUserService.Email);
        var userRoleNames = await _userManager.GetRolesAsync(currentUser);
        var userRoles = await _roleManager.Roles.Where(r => userRoleNames.Contains(r.Name)).ToListAsync();
        foreach (var userRole in userRoles)
        {
            claims.AddRange(await _roleManager.GetClaimsAsync(userRole));
        }
        return await Task.FromResult(claims.Select(c => c.Value).Distinct());
    }

    public async Task<bool> HasPermission(string permission)
    {
        return (await GetUserPermissions()).Any(c => c == permission);
    }

    private async Task<string[]> GetRoles()
    {
        var claimsPrincipal = _currentUserService.User;
        if (claimsPrincipal == null) throw new ArgumentNullException(nameof(claimsPrincipal));

        var nameIdentifier = _currentUserService.UserId;
        var usersExist = _userRepository.Query().Any();

        if (!usersExist)
            await SeedRootUser(nameIdentifier);

        var roles = _roleManager.Roles.Select(r => r.Name).ToArray();

        if (roles.Length == 0)
            throw new NoRolesAssignedException();

        return roles;
    }

    private async Task SeedRootUser(string userId)
    {
        var rootUser = new PostUserDto()
        {
            Username = _currentUserService.Username,
            Email = _currentUserService.Email,
            FirstName = _currentUserService.FirstName,
            LastName = _currentUserService.LastName
        };

        var userCommand = new AddUser.Command(rootUser, true);
        var createdUser = await _mediator.Send(userCommand);

        var roleCommand = new AddUserRole.Command(createdUser.Id, "Admin", true);
        await _mediator.Send(roleCommand);

    }
}