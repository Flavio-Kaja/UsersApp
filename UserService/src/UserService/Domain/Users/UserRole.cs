namespace UserService.Domain.Users;

using UserService.Domain.Users.DomainEvents;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Roles;
using Microsoft.AspNetCore.Identity;

public class UserRole : IdentityUserRole<Guid>, IDomainEventable
{
    public User User { get; set; }
    public Role Role { get; set; }

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new List<DomainEvent>();

    public void QueueDomainEvent(DomainEvent @event)
    {
        if (!DomainEvents.Contains(@event))
            DomainEvents.Add(@event);
    }

    public static UserRole Create(Guid userId, Guid roleId)
    {
        var newUserRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        newUserRole.QueueDomainEvent(new UserRolesUpdated() { UserId = userId });

        return newUserRole;
    }

    protected UserRole() { } // For EF + Mocking
}