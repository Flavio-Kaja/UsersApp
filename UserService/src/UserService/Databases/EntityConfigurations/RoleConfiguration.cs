using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Roles;
using UserService.Domain.Users;

namespace UserService.Databases.EntityConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        /// <summary>
        /// The db configuration for the Users. 
        /// </summary>
        public void Configure(EntityTypeBuilder<Role> builder)
        {
        }
    }
}