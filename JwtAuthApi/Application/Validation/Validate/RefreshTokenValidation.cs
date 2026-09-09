using System.Text.Json;
using Application.DTOs;

namespace Application.Validation.Validate;

public static class RefreshTokenValidation
{
    public static ValidationResult ValidateRefreshToken(RefreshRequestDto dto) =>
        new Validator<RefreshRequestDto>(dto)
            .RuleFor("RefreshToken", x => x.RefreshToken)
            .Must(v => v.ValueKind == JsonValueKind.String &&
                       !string.IsNullOrEmpty(v.GetString()),
                "RefreshToken must be a non-empty string")
            .MinLength(88)
            .MaxLength(88)
            .Validate();
}