namespace UserService.Databases.EntityConfigurations;

using Domain.Users;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// The database configuration for UserRoles. 
    /// </summary>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasOne(e => e.User)
        .WithMany(e => e.UserRoles)
        .HasForeignKey(ur => ur.UserId);

        builder.HasOne(e => e.Role)
        .WithMany(e => e.UserRoles)
        .HasForeignKey(ur => ur.RoleId);
    }
}