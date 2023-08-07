using FluentValidation;
using System.ComponentModel.DataAnnotations;
using UserService.Domain.Users.Dtos;

namespace UserService.Authentication.Models
{
    public class UserLoginModel
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class UserLoginValidator : AbstractValidator<UserLoginModel>
    {
        public UserLoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Length(6, 50).WithMessage("Password must be between 6 and 50 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one capital letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one number.")
                .Matches(@"[!@#$%^&*(),.?""{}|<>]").WithMessage("Password must contain at least one special character.");

        }
    }

}
