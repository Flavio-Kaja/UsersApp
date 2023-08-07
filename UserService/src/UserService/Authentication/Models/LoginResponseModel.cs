using System.IdentityModel.Tokens.Jwt;

namespace UserService.Authentication.Models
{
    /// <summary>
    /// Login response model
    /// </summary>
    public class LoginResponseModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="token">Security token</param>
        /// <param name="expiration">The date the <see cref="Token"/> expires</param>
        public LoginResponseModel(JwtSecurityToken token = default)
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Gets the security token
        /// </summary>
        public string Token { get; }

    }
}
