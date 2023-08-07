using Grpc.Core;
using MediatR;
using Microsoft.VisualBasic;
using System.Data.Entity;
using System.Threading;
using UserService.Domain.Users;
using UserService.Domain.Users.Mappings;
using UserService.Domain.Users.Services;
using UserService.Protos;
using UserService.Wrappers;

namespace UserService.Services;

public class GrpcUserService : Protos.UserService.UserServiceBase
{
    private readonly IUserRepository _userRepository;

    public GrpcUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async override Task<UserListResponse> GetUserList(UserParametersRequest request, ServerCallContext context)
    {

        var collection = _userRepository.Query().AsNoTracking();

        //filtering 
        if (request.UserIds.Any())
        {
            List<Guid> guidIds = request.UserIds
            .Select(s => Guid.TryParse(s, out var guid) ? guid : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g.Value)
            .ToList();

            collection = collection.Where(u => guidIds.Contains(u.Id));
        }
        if (!string.IsNullOrEmpty(request.FirstName))
        {
            collection = collection.Where(u => u.FirstName.ToLower().Contains(request.FirstName));
        }
        if (!string.IsNullOrEmpty(request.LastName))
        {
            collection = collection.Where(u => u.LastName.ToLower().Contains(request.LastName));
        }
        if (!string.IsNullOrEmpty(request.Email))
        {
            collection = collection.Where(u => u.Email.ToLower().Contains(request.Email));
        }
        if (request.MinDailyGoal != default)
        {
            collection = collection.Where(u => u.DailyGoal >= request.MinDailyGoal);
        }
        if (request.MaxDailyGoal != default)
        {
            collection = collection.Where(u => u.DailyGoal <= request.MaxDailyGoal);
        }

        // Ordering
        if (StringComparer.OrdinalIgnoreCase.Equals(request.SortBy, nameof(User.FirstName)))
        {
            collection = request.Descending
                ? collection.OrderByDescending(user => user.FirstName)
                : collection.OrderBy(user => user.FirstName);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(request.SortBy, nameof(User.LastName)))
        {
            collection = request.Descending
                ? collection.OrderByDescending(user => user.LastName)
                : collection.OrderBy(user => user.LastName);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(request.SortBy, nameof(User.Email)))
        {
            collection = request.Descending
                ? collection.OrderByDescending(user => user.Email)
                : collection.OrderBy(user => user.Email);
        }
        if (StringComparer.OrdinalIgnoreCase.Equals(request.SortBy, nameof(User.DailyGoal)))
        {
            collection = request.Descending
                ? collection.OrderByDescending(user => user.DailyGoal)
                : collection.OrderBy(user => user.DailyGoal);
        }

        var dtoCollection = collection.ToUserDtoQueryable();

        var queryResponse = await PagedList<Domain.Users.Dtos.UserDto>.CreateAsync(dtoCollection,
            request.PageNumber,
            request.PageSize, new CancellationToken());

        var response = new UserListResponse
        {
            TotalCount = queryResponse.TotalCount,
            PageNumber = queryResponse.PageNumber,
            PageSize = queryResponse.PageSize
        };

        foreach (var user in queryResponse)
        {
            var protoUser = new Protos.UserDto
            {
                Id = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                DailyGoal = user.DailyGoal
            };
            response.Users.Add(protoUser);
        }

        return response;
    }
}

