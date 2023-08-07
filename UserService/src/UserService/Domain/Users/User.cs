using UserService.Exceptions;

namespace UserService.Domain.Users;

using UserService.Domain.Users.Dtos;
using UserService.Domain.Users.DomainEvents;
using Roles;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using Sieve.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using ValidationException = ValidationException;

public class User : IdentityUser<Guid>, IAuditable, ISoftDeletable, IDomainEventable
{
    // User properties
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    [NotMapped]
    public string FullName => FirstName + " " + LastName;
    // Goals properties
    public int DailyGoal { get; private set; }

    [JsonIgnore]
    [IgnoreDataMember]
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    // Auditable properties
    public DateTime CreatedOn { get; private set; }

    public string CreatedBy { get; private set; }

    public DateTime? LastModifiedOn { get; private set; }

    public string LastModifiedBy { get; private set; }

    public bool IsDeleted { get; private set; }

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new List<DomainEvent>();


    public static User Create(PostUserDto userForCreation)
    {
        ValidationException.ThrowWhenNullOrWhitespace(userForCreation.Username,
            "Please provide a username.");

        var newUser = new User
        {
            FirstName = userForCreation.FirstName,
            LastName = userForCreation.LastName,
            Email = userForCreation.Email,
            UserName = userForCreation.Username,
            DailyGoal = userForCreation.DailyGoal,
        };

        newUser.QueueDomainEvent(new UserCreated() { User = newUser });
        return newUser;
    }

    public User Update(PostUserDto userForUpdate)
    {
        ValidationException.ThrowWhenNullOrWhitespace(userForUpdate.Username,
            "Please provide a username.");

        FirstName = userForUpdate.FirstName;
        LastName = userForUpdate.LastName;
        UserName = userForUpdate.Username;
        DailyGoal = userForUpdate.DailyGoal;
        QueueDomainEvent(new UserUpdated() { Id = Id });
        return this;
    }

    public UserRole AddRole(Role role)
    {
        var newList = UserRoles.ToList();
        var userRole = UserRole.Create(Id, role.Id);
        newList.Add(userRole);
        UpdateRoles(newList);
        return userRole;
    }

    public UserRole RemoveRole(Role role)
    {
        var newList = UserRoles.ToList();
        var roleToRemove = UserRoles.FirstOrDefault(x => x.RoleId == role.Id);
        newList.Remove(roleToRemove);
        UpdateRoles(newList);
        return roleToRemove;
    }

    private void UpdateRoles(IList<UserRole> updates)
    {
        var additions = updates.Where(userRole => UserRoles.All(x => x.RoleId != userRole.RoleId)).ToList();
        var removals = UserRoles.Where(userRole => updates.All(x => x.RoleId != userRole.RoleId)).ToList();

        var newList = UserRoles.ToList();
        removals.ForEach(toRemove => newList.Remove(toRemove));
        additions.ForEach(newRole => newList.Add(newRole));
        UserRoles = newList;
        QueueDomainEvent(new UserRolesUpdated() { UserId = Id });
    }

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
    public User() { }
}