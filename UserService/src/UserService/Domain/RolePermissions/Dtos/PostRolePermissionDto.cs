using FluentValidation;

namespace UserService.Domain.RolePermissions.Dtos;

public sealed class PostRolePermissionDto
{
    public string Role { get; set; }
    public string Permission { get; set; }
}

public class PostRolePermissionDtoValidator : AbstractValidator<PostRolePermissionDto>
{
    public PostRolePermissionDtoValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Length(3, 50).WithMessage("Role must be between 3 and 50 characters.");

        RuleFor(x => x.Permission)
            .NotEmpty().WithMessage("Permission is required.")
            .Length(3, 50).WithMessage("Permission must be between 3 and 50 characters.");
    }
}