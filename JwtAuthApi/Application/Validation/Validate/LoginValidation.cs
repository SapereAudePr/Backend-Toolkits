using Application.DTOs;

namespace Application.Validation.Validate;

public class LoginValidation
{
    public static ValidationResult ValidateLogin(LoginDto dto) =>
        new Validator<LoginDto>(dto)
            .RuleFor("Name", x => x.Name)
            .NotNull()
            .NotEmpty()
            .RuleFor("Password", x => x.Password)
            .NotNull()
            .NotEmpty()
            .Validate();
}