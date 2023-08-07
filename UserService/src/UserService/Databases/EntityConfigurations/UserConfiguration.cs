namespace UserService.Databases.EntityConfigurations;

using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserConfiguration : BaseEntityConfiguration<User>
{
    /// <summary>
    /// The db configuration for the Users. 
    /// </summary>
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        builder.Property(u => u.DailyGoal).IsRequired(true).HasDefaultValue(20);
        builder.Property(u => u.LastName).IsRequired(true).HasMaxLength(200);
        builder.Property(u => u.FirstName).IsRequired(true).HasMaxLength(200);
    }
}