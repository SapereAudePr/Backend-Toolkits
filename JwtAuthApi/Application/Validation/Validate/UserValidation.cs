using Application.DTOs;

namespace Application.Validation.Validate;

public class UserValidation
{
    public static ValidationResult ValidateUserCreation(CreateUserDto userDto)
        => new Validator<CreateUserDto>(userDto)
            .RuleFor("Name", x => x.Name)
            .NotNull()
            .NotEmpty()
            .MinLength(2)
            .MaxLength(40)
            .RuleFor("Password", x => x.Password)
            .NotNull()
            .NotEmpty()
            .MinLength(8)
            .MaxLength(100)
            .RuleFor("CreatedBy", x => x.CreatedBy)
            .NotNull()
            .NotEmpty()
            .MinLength(2)
            .MaxLength(40)
            .Validate();

    public static ValidationResult ValidateUserUpdate(UpdateUserDto userDto)
        => new Validator<UpdateUserDto>(userDto)
            .RuleFor("Name", x => x.Name)
            .NotNull()
            .NotEmpty()
            .MinLength(2)
            .MaxLength(40)
            .RuleFor("Password", x => x.Password)
            .NotNull()
            .NotEmpty()
            .MinLength(8)
            .MaxLength(100)
            .Validate();
    
    public static ValidationResult ValidateUserPatch(PatchUserDto userDto)
        => new Validator<PatchUserDto>(userDto)
            .RuleFor("Name", x => x.Name)
            .NotNull()
            .NotEmpty()
            .MinLength(2)
            .MaxLength(40)
            .RuleFor("Password", x => x.Password)
            .NotNull()
            .NotEmpty()
            .MinLength(8)
            .MaxLength(100)
            .Validate();
}