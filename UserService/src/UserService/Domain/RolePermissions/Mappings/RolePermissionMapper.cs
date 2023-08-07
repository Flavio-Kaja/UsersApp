namespace UserService.Domain.RolePermissions.Mappings;

using UserService.Domain.RolePermissions.Dtos;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class RolePermissionMapper
{
    public static partial RolePermissionDto ToRolePermissionDto(this RolePermission rolePermission);
    public static partial IQueryable<RolePermissionDto> ToRolePermissionDtoQueryable(this IQueryable<RolePermission> rolePermission);
}