namespace UserService.Authentication.Models
{
    public class AuthenticatedUser
    {
        public AuthenticatedUser()
        {
        }

        public AuthenticatedUser(Guid id, string name, string email, string role, List<string> permissions)
        {
            Id = id;
            Name = name;
            Email = email;
            Role = role;
            Permissions = permissions;
        }

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the role
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public List<string> Permissions { get; set; }
    }

}
