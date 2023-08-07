using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UserService.Domain;
using UserService.Domain.Roles;
using UserService.Domain.Users;
using UserService.Domain.Users.Dtos;
using UserService.Services;

namespace UserService.Databases;

public interface ISeedDataService : IUserServiceScopedService
{
    public Task Initialize();
}

public class SeedDataService : ISeedDataService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly string SeedPw = "2d2o!jr0rjrQ";

    public SeedDataService(RoleManager<Role> roleManager, UserManager<User> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task Initialize()
    {
        var permissions = Permissions.List();
        var userPermissions = Permissions.GetUserPermissions();

        // Seed the 'Admin' role
        if (!_roleManager.Roles.Any(r => r.Name == "Admin"))
        {
            var adminRole = new Role("Admin");

            var roleResult = await _roleManager.CreateAsync(adminRole);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create 'Admin' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            foreach (var permission in permissions)
            {
                await _roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim(ClaimTypes.AuthorizationDecision, permission));
            }
        }

        // Seed the 'User' role
        if (!_roleManager.Roles.Any(r => r.Name == "Admin"))
        {
            var userRole = new Role("User");
            var roleResult = await _roleManager.CreateAsync(userRole);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create 'User' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
            foreach (var permission in userPermissions)
            {
                await _roleManager.AddClaimAsync(userRole, new System.Security.Claims.Claim(ClaimTypes.AuthorizationDecision, permission));
            }
        }
        // Seed the 'Admin' user
        if (!_userManager.Users.Any(u => u.UserName == "admin"))
        {
            var adminUserDto = new PostUserDto
            {
                FirstName = "admin",
                LastName = "admin",
                Username = "admin",
                DailyGoal = 20,
                Email = "admin@example.com",
            };

            var adminUser = User.Create(adminUserDto);
            var userResult = await _userManager.CreateAsync(adminUser, SeedPw);
            if (userResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new InvalidOperationException($"Unable to create 'Admin' user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
            }
        }

        // Seed the 'User' user
        if (!_userManager.Users.Any(u => u.UserName == "user"))
        {
            var standartUserDto = new PostUserDto
            {
                FirstName = "user",
                LastName = "user",
                Username = "standartuser",
                DailyGoal = 20,
                Email = "user@example.com",
            };

            var standartUser = User.Create(standartUserDto);
            var userResult = await _userManager.CreateAsync(standartUser, SeedPw);
            if (userResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(standartUser, "User");
            }
            else
            {
                throw new InvalidOperationException($"Unable to create 'User' user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
