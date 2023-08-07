namespace UserService.Domain.Users.Features;

using UserService.Domain.Users.Dtos;
using UserService.Domain.Users.Services;
using UserService.Wrappers;
using UserService.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;

public static class GetUserList
{
    public sealed class Query : IRequest<PagedList<UserDto>>
    {
        public readonly UserParametersDto QueryParameters;

        public Query(UserParametersDto queryParameters)
        {
            QueryParameters = queryParameters;
        }
    }

    public sealed class Handler : IRequestHandler<Query, PagedList<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PagedList<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {

            var collection = _userRepository.Query().AsNoTracking();

            //filtering 
            if (!string.IsNullOrEmpty(request.QueryParameters.FirstName))
            {
                collection = collection.Where(u => u.FirstName.ToLower().Contains(request.QueryParameters.FirstName));
            }
            if (!string.IsNullOrEmpty(request.QueryParameters.LastName))
            {
                collection = collection.Where(u => u.LastName.ToLower().Contains(request.QueryParameters.LastName));
            }
            if (!string.IsNullOrEmpty(request.QueryParameters.Email))
            {
                collection = collection.Where(u => u.Email.ToLower().Contains(request.QueryParameters.Email));
            }
            if (request.QueryParameters.MinDailyGoal != null)
            {
                collection = collection.Where(u => u.DailyGoal >= request.QueryParameters.MinDailyGoal);
            }
            if (request.QueryParameters.MaxDailyGoal != null)
            {
                collection = collection.Where(u => u.DailyGoal <= request.QueryParameters.MaxDailyGoal);
            }

            // Ordering
            if (StringComparer.OrdinalIgnoreCase.Equals(request.QueryParameters.SortBy, nameof(User.FirstName)))
            {
                collection = request.QueryParameters.Descending
                    ? collection.OrderByDescending(user => user.FirstName)
                    : collection.OrderBy(user => user.FirstName);
            }
            if (StringComparer.OrdinalIgnoreCase.Equals(request.QueryParameters.SortBy, nameof(User.LastName)))
            {
                collection = request.QueryParameters.Descending
                    ? collection.OrderByDescending(user => user.LastName)
                    : collection.OrderBy(user => user.LastName);
            }
            if (StringComparer.OrdinalIgnoreCase.Equals(request.QueryParameters.SortBy, nameof(User.Email)))
            {
                collection = request.QueryParameters.Descending
                    ? collection.OrderByDescending(user => user.Email)
                    : collection.OrderBy(user => user.Email);
            }
            if (StringComparer.OrdinalIgnoreCase.Equals(request.QueryParameters.SortBy, nameof(User.DailyGoal)))
            {
                collection = request.QueryParameters.Descending
                    ? collection.OrderByDescending(user => user.DailyGoal)
                    : collection.OrderBy(user => user.DailyGoal);
            }

            var dtoCollection = collection.ToUserDtoQueryable();

            return await PagedList<UserDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}