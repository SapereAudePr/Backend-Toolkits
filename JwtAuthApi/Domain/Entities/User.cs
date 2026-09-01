using Application.Common;

namespace Domain.Entities;

public class User
{
    public int Id { get; }

    public string Name { get; private set; } = null!;

    public string HashedPassword { get; private set; } = null!;

    public DateTimeOffset CreationTime { get; init; }

    public string CreatedBy { get; private set; } = null!;

    private User()
    {
    }

    public User(string name, string hashedPassword, string createdBy)
    {
        Name = ChangeName(name);
        HashedPassword = SetHashedPassword(hashedPassword);
        CreationTime = DateTimeOffset.UtcNow;
        CreatedBy = ChangeCreator(createdBy);
    }

    public string ChangeName(string name)
    {
        Name = name.CheckNullOrWhiteSpace(trimValue: true).MinLength(2).MaxLength(40);

        return Name;
    }
    
    // Password has to sent as hashed. This class can not hash password thus there's no way
    // of creating a user by domain with hashed password
    // I'll maybe move IPasswordHasher to Domain or find another solution
    public string SetHashedPassword(string hashedPassword)
    {
        HashedPassword = hashedPassword.CheckNullOrWhiteSpace().MaxLength(100);

        return hashedPassword;
    }

    public string ChangeCreator(string name)
    {
        CreatedBy = name.CheckNullOrWhiteSpace(trimValue: true).MinLength(2).MaxLength(40);

        return name;
    }
}