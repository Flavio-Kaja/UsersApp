namespace UserService.Domain.Roles;

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Users;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

public class Role : IdentityRole<Guid>, IAuditable, IDomainEventable, ISoftDeletable

{

    public DateTime CreatedOn { get; private set; }

    public string CreatedBy { get; private set; }

    public DateTime? LastModifiedOn { get; private set; }

    public string LastModifiedBy { get; private set; }

    public bool IsDeleted { get; private set; }

    [JsonIgnore]
    [IgnoreDataMember]
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();


    public Role(string value)
    {
        Name = value;
    }

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new List<DomainEvent>();

    public void UpdateCreationProperties(DateTime createdOn, string createdBy)
    {
        CreatedOn = createdOn;
        CreatedBy = createdBy;
    }

    public void UpdateModifiedProperties(DateTime? lastModifiedOn, string lastModifiedBy)
    {
        LastModifiedOn = lastModifiedOn;
        LastModifiedBy = lastModifiedBy;
    }

    public void UpdateIsDeleted(bool isDeleted)
    {
        IsDeleted = isDeleted;
    }

    public void QueueDomainEvent(DomainEvent @event)
    {
        if (!DomainEvents.Contains(@event))
            DomainEvents.Add(@event);
    }

    protected Role() { } // EF Core
}
