using System.Text.Json;

namespace Application.DTOs;

public class RefreshRequestDto
{
    public JsonElement RefreshToken { get; set; }
}