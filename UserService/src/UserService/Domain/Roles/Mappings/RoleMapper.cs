

using Riok.Mapperly.Abstractions;
using UserService.Domain.Roles.Dtos;
using UserService.Domain.Users;

namespace UserService.Domain.Roles.Mappings
{
    /// <summary>
    /// Role mapping class
    /// </summary>
    [Mapper]
    public static partial class RoleMapper
    {
        public static partial RoleDto ToRoleDto(this Role role);
        public static partial IQueryable<RoleDto> ToRoleDtoQueryable(this IQueryable<Role> user);
    }
}
