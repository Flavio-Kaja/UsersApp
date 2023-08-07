using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using UserService.Domain;

namespace UserService.Databases.EntityConfigurations
{
    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : class, IAuditable, ISoftDeletable
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.CreatedOn)
                .IsRequired();

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(e => e.LastModifiedOn)
                .IsRequired(false);

            builder.Property(e => e.LastModifiedBy)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
