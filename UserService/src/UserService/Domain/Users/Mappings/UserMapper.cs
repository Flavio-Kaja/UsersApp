namespace UserService.Domain.Users.Mappings;

using UserService.Domain.Users.Dtos;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class UserMapper
{
    public static partial UserDto ToUserDto(this User user);
    public static partial IQueryable<UserDto> ToUserDtoQueryable(this IQueryable<User> user);
}