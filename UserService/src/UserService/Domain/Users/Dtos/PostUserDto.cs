using FluentValidation;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace UserService.Domain.Users.Dtos;

public sealed class PostUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public int DailyGoal { get; set; }
    public string Password { get; set; }

}

public class PostUserDtoValidator : AbstractValidator<PostUserDto>
{
    public PostUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(1, 100).WithMessage("First name must be between 1 and 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(1, 100).WithMessage("Last name must be between 1 and 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Please provide a correct email address")
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(4, 50).WithMessage("Username must be between 4 and 50 characters.");

        RuleFor(x => x.DailyGoal)
            .GreaterThan(0).WithMessage("Daily goal must be greater than 0.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(6, 50).WithMessage("Password must be between 6 and 50 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one capital letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one number.")
            .Matches(@"[!@#$%^&*(),.?""{}|<>]").WithMessage("Password must contain at least one special character.");
    }
}
