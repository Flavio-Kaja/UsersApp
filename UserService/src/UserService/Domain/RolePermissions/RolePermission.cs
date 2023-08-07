namespace UserService.Domain.RolePermissions;

using Dtos;
using DomainEvents;
using Roles;
using Domain;
using UserService.Exceptions;

public class RolePermission : BaseEntity
{
    public Role Role { get; private set; }
    public string Permission { get; private set; }


    public static RolePermission Create(PostRolePermissionDto rolePermissionForCreation)
    {
        ValidationException.Must(BeAnExistingPermission(rolePermissionForCreation.Permission),
            "Please use a valid permission.");

        var newRolePermission = new RolePermission
        {
            Role = new Role(rolePermissionForCreation.Role),
            Permission = rolePermissionForCreation.Permission
        };

        newRolePermission.QueueDomainEvent(new RolePermissionCreated() { RolePermission = newRolePermission });

        return newRolePermission;
    }

    public RolePermission Update(PostRolePermissionDto rolePermissionForUpdate)
    {
        ValidationException.Must(BeAnExistingPermission(rolePermissionForUpdate.Permission),
            "Please use a valid permission.");

        Role = new Role(rolePermissionForUpdate.Role);
        Permission = rolePermissionForUpdate.Permission;

        QueueDomainEvent(new RolePermissionUpdated() { Id = Id });
        return this;
    }

    private static bool BeAnExistingPermission(string permission)
    {
        return Permissions.List().Contains(permission, StringComparer.InvariantCultureIgnoreCase);
    }

    protected RolePermission() { } // For EF + Mocking
}