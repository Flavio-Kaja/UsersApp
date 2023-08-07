namespace UserService.Domain.Users.Services;

using Microsoft.EntityFrameworkCore;
using UserService.Domain.Users;
using UserService.Databases;
using UserService.Services;

public interface IUserRepository : IGenericRepository<User>
{
    public Task AddRole(UserRole entity, CancellationToken cancellationToken = default);
    public void RemoveRole(UserRole entity);
}

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly UserDbContext _dbContext;

    public UserRepository(UserDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<User> GetByIdOrDefault(Guid id, bool withTracking = true, CancellationToken cancellationToken = default)
    {
        return withTracking
            ? await _dbContext.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            : await _dbContext.Users
                .Include(u => u.UserRoles)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddRole(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserRoles.AddAsync(userRole, cancellationToken);
    }

    public void RemoveRole(UserRole userRole)
    {
        _dbContext.UserRoles.Remove(userRole);
    }
}
