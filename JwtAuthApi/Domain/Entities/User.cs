using Application.Common;

namespace Domain.Entities;

public class User
{
    public int Id { get; }

    public string Name { get; private set; } = null!;

    public string Password { get; private set; } = null!;

    public DateTimeOffset CreationTime { get; init; }

    public string CreatedBy { get; private set; } = null!;

    private User()
    {
    }

    public User(string name, string password, string createdBy)
    {
        Name = ChangeName(name);
        Password = ChangePassword(password);
        CreationTime = DateTimeOffset.UtcNow;
        CreatedBy = ChangeCreator(createdBy);
    }

    public string ChangeName(string name)
    {
        Name = name.CheckNullOrWhiteSpace(trimValue: true).MinLength(2).MaxLength(40);

        return Name;
    }

    public string ChangePassword(string password)
    {
        Password = password.CheckNullOrWhiteSpace().MinLength(8).MaxLength(100);

        return password;
    }

    public string ChangeCreator(string name)
    {
        CreatedBy = name.CheckNullOrWhiteSpace(trimValue: true).MinLength(2).MaxLength(40);

        return name;
    }
}