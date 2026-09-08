using Application.DTOs;

namespace Application.Validation.Validate;

public class RefreshTokenValidation
{
    public static ValidationResult ValidateRefreshToken(RefreshRequestDto dto) =>
        new Validator<RefreshRequestDto>(dto)
            .RuleFor("RefreshToken", x => x.RefreshToken)
            .NotNull()
            .NotEmpty()
            .Validate();
}