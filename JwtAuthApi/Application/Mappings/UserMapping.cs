using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings;

public static class UserMapping
{
    public static List<UserDto> ToDto(this List<User> users) => users.Select(user =>
        user.ToDto()).ToList();


    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        CreatedBy = user.CreatedBy
    };

    public static User ToDomain(this CreateUserDto userDto) => new(
        name: userDto.Name,
        hashedPassword: userDto.Password,
        createdBy: userDto.CreatedBy);
}