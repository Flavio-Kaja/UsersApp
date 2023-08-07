
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Authentication.Models;
using UserService.Domain.Roles;
using UserService.Domain.Users;
using UserService.Services;

namespace UserService.Authentication.Services
{
    public interface IAuthenticationService : IUserServiceScopedService
    {
        Task<JwtSecurityToken> AuthenticateAsync(User user, CancellationToken cancellationToken = default);
        Task SignOutAsync(int userId, CancellationToken cancellationToken = default);
        Task<AuthenticatedUser> GetAuthenticatedUserAsync(CancellationToken cancellationToken = default);
        Task SignClientOutAsync(int clientId, CancellationToken cancellationToken = default);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TokenSettings _settings;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AuthenticationService(IHttpContextAccessor httpContextAccessor, IDistributedCache distributedCache, ILogger<AuthenticationService> logger, UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _distributedCache = distributedCache;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _settings = new TokenSettings(_configuration["Auth:ClientSecret"], _configuration["Auth:Issuer"]);
        }

        /// <summary>
        /// Set user claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected async Task<List<Claim>> SetClaimsAsync(User user)
        {
            List<Claim> claims = new();
            var roles = await _userManager.GetRolesAsync(user);
            Claim idClaim = new(ClaimTypes.NameIdentifier, user.Id.ToString());
            Claim jtiClaim = new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
            Claim nameClaim = new(ClaimTypes.Name, user.FullName);
            Claim emailClaim = new(ClaimTypes.Email, user.Email);
            Claim roleClaim = new(ClaimTypes.GroupSid, roles?.First() ?? string.Empty);
            List<Claim> roleClaims = await GetUserClaimsAsync(user);
            Claim dailyGoalClaim = new("DailyGoal", user.DailyGoal.ToString());

            claims.AddRange(new List<Claim> { idClaim, jtiClaim, nameClaim, emailClaim, roleClaim, dailyGoalClaim });
            claims.AddRange(roleClaims);

            return claims;
        }

        /// <summary>
        /// Get the user claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<List<Claim>> GetUserClaimsAsync(User user)
        {

            var claims = new List<Claim>();
            var roles = await _userManager.GetRolesAsync(user);

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);

                if (role != null)
                {
                    await GetRoleClaims(claims, role);
                }
            }

            return claims;
        }

        /// <summary>
        /// Get role claims
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private async Task GetRoleClaims(List<Claim> claims, Role role)
        {

            // Get claims for the role
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            claims.AddRange(roleClaims);
        }
        /// <summary>
        /// Check if the httopcontext is correctly initialized
        /// </summary>
        /// <returns></returns>
        protected bool IsRequestAvailable()
        {
            try
            {
                return _httpContextAccessor?.HttpContext != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to access the HttpContext.");
                return false;
            }
        }

        /// <summary>
        /// Generate the jwt token
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        protected virtual JwtSecurityToken GenerateToken(List<Claim> claims)
        {
            string key = _settings.EncryptionKey;
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(key));
            JwtSecurityToken token = new(

                expires: _settings.Expiration,
                claims: claims,
                issuer: _settings.Issuer,
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        /// <summary>
        /// Generate authentication token for user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<JwtSecurityToken> AuthenticateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            List<Claim> claims = await SetClaimsAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            AuthenticatedUser authenticatedUser = new(user.Id, user.FullName, user.Email,
                roles?.First() ?? string.Empty,
            (await GetUserClaimsAsync(user)).Select(r => r.Value).ToList() ?? new List<string>());

            await CacheAuthenticatedUser(user.Id.ToString(), authenticatedUser, cancellationToken);

            JwtSecurityToken token = GenerateToken(claims);
            return token;
        }

        /// <summary>
        /// Cache the authenticated user
        /// </summary>
        /// <param name="key"></param>
        /// <param name="authenticatedUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CacheAuthenticatedUser(string key, AuthenticatedUser authenticatedUser, CancellationToken cancellationToken)
        {
            var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20));
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(authenticatedUser), cacheEntryOptions, cancellationToken);
        }

        public async Task SignOutAsync(int userId, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(userId.ToString(), cancellationToken);
        }

        public async Task<AuthenticatedUser> GetAuthenticatedUserAsync(CancellationToken cancellationToken = default)
        {
            var cachedUserString = await _distributedCache.GetStringAsync(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cachedUserString))
            {
                return JsonConvert.DeserializeObject<AuthenticatedUser>(cachedUserString);
            }
            if (_httpContextAccessor.HttpContext?.User.Identity is ClaimsIdentity identity)
            {
                try
                {
                    if (identity.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                        return null;

                    AuthenticatedUser user = new()
                    {
                        Id = Guid.Parse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                        Name = identity.FindFirst(ClaimTypes.Name)?.Value,
                        Email = identity.FindFirst(ClaimTypes.Email)?.Value,
                        Role = identity.FindFirst(ClaimTypes.Role)?.Value,
                        Permissions = identity.FindAll(ClaimTypes.AuthorizationDecision).Select(c => c.Value).ToList()
                    };

                    await CacheAuthenticatedUser(user.Id.ToString(), user, cancellationToken);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return null;

        }
        /// <summary>
        /// Remove cached client from distributed cache
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SignClientOutAsync(int clientId, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(clientId.ToString(), cancellationToken);
        }
    }

}
