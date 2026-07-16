using System;
using Validation;
using Xunit;
using Xunit.Abstractions;

namespace Test;

internal record Person(string Name, int Age);

internal record Email(string Value);

public class ValidationTest(ITestOutputHelper output)
{
    [Fact]
    public void NotEmpty_WhenEmpty_AddsError()
    {
        var validation = new Validator<Person>(new Person("", 22))
            .RuleFor("Name", x => x.Name)
            .NotEmpty()
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.Contains(validation.Errors, x => x.Field == "Name");
        Assert.Single(validation.Errors);
        Assert.False(validation.IsValid);
    }

    [Fact]
    public void NotEmpty_WhenNotEmpty_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 20))
            .RuleFor("Name", x => x.Name)
            .NotEmpty()
            .Validate();

        Assert.Empty(validation.Errors);
        Assert.True(validation.IsValid);
    }

    [Fact]
    public void NotNull_WhenNull_AddsError()
    {
        string? nullName = null;

        var validation =
            new Validator<Person>(new Person(nullName!, 20))
                .RuleFor("Name", x => x.Name)
                .NotNull()
                .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Name");
    }

    [Fact]
    public void MinLength_WhenLowerThanMin_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 20))
            .RuleFor("Name", x => x.Name)
            .MinLength(18)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Name");
    }

    [Fact]
    public void MinLength_WhenHigherThanMin_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 25))
            .RuleFor("Name", x => x.Name)
            .MinLength(1)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void MaxLength_WhenHigherThanMax_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 25))
            .RuleFor("Name", x => x.Name)
            .MaxLength(3)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Name");
    }

    [Fact]
    public void MaxLength_WhenLowerThanMax_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 26))
            .RuleFor("Name", x => x.Name)
            .MaxLength(10)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void Min_WhenLowerThanMin_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 26))
            .RuleFor("Age", x => x.Age)
            .Min(27)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Age");
    }

    [Fact]
    public void Min_WhenHigherThanMin_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 27))
            .RuleFor("Age", x => x.Age)
            .Min(24)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }


    [Fact]
    public void Max_WhenHigherThanMax_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 28))
            .RuleFor("Age", x => x.Age)
            .Max(27)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Age");
    }

    [Fact]
    public void Max_WhenLowerThanMax_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 24))
            .RuleFor("Age", x => x.Age)
            .Max(27)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));


        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void GreaterThan_WhenLower_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 17))
            .RuleFor("Age", x => x.Age)
            .GreaterThan(18)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Age");
    }

    [Fact]
    public void GreaterThan_WhenGreater_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 20))
            .RuleFor("Age", x => x.Age)
            .GreaterThan(18)
            .Validate();

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void LessThan_WhenGreater_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 30))
            .RuleFor("Age", x => x.Age)
            .LessThan(25)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Age");
    }

    [Fact]
    public void LessThan_WhenLower_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 20))
            .RuleFor("Age", x => x.Age)
            .LessThan(25)
            .Validate();

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void Equal_WhenNotEqual_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 76))
            .RuleFor("Name", x => x.Name)
            .Equal("Guy")
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));


        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Name");
    }

    [Fact]
    public void Equal_WhenEqual_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 21))
            .RuleFor("Name", x => x.Name)
            .Equal("John")
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void NotEqual_WhenEqual_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 19))
            .RuleFor("Name", x => x.Name)
            .NotEqual("John")
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Name");
    }

    [Fact]
    public void NotEqual_WhenNotEqual_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 16))
            .RuleFor("Name", x => x.Name)
            .NotEqual("Raven")
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void Must_WhenDoesNotMetRules_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 25))
            .RuleFor("Name", x => x.Name)
            .Must(name => name.Contains('M'), "Name must contains \"M\"")
            .Must(name => name.StartsWith('A'))
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.Equal(2, validation.Errors.Count);
        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, x => x.Field == "Name");
    }

    [Fact]
    public void Must_WhenMetRules_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 26))
            .RuleFor("Name", x => x.Name)
            .Must(name => name.EndsWith('n'))
            .Validate();

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Theory]
    [InlineData("dwwfgqw.gmail")]
    [InlineData("test..1@24gmail.com")]
    [InlineData("test1!24@yahoo.com")]
    public void EmailAddress_WhenNotValid_AddsError(string email)
    {
        var validation = new Validator<Email>(new Email(email))
            .RuleFor("Value", x => x.Value)
            .EmailAddress()
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.Single(validation.Errors);
        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, x => x.Field == "Value");
    }

    [Theory]
    [InlineData("john@gmail.com")]
    [InlineData("test123@yahoo.com")]
    [InlineData("alice.smith@hotmail.com")]
    public void EmailAddress_WhenValid_DoesNotAddError(string email)
    {
        var validation = new Validator<Email>(new Email(email))
            .RuleFor("Value", x => x.Value)
            .EmailAddress()
            .Validate();

        Assert.Empty(validation.Errors);
        Assert.True(validation.IsValid);
    }

    [Fact]
    public void Matches_WhenNotValid_AddsError()
    {
        var validation = new Validator<Person>(new Person("John", 21))
            .RuleFor("Name", x => x.Name)
            .Matches(@"^A\w+")
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Single(validation.Errors);
        Assert.Contains(validation.Errors, x => x.Field == "Name");
    }

    [Fact]
    public void Matches_WhenValid_DoesNotAddError()
    {
        var validation = new Validator<Person>(new Person("John", 271))
            .RuleFor("Name", x => x.Name)
            .Matches(@"^J\w+")
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void ValidationTest_WhenAllFieldsInvalid_AddsError()
    {
        var validation = new Validator<Person>(new Person("", 140))
            .RuleFor("Name", x => x.Name)
            .NotEmpty()
            .MinLength(10)
            .Equal("Hannah")
            .Matches(@"^b\d{8}")
            .Must(x => x.StartsWith('A'))
            .RuleFor("Age", x => x.Age)
            .LessThan(120)
            .Max(100)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Equal(7, validation.Errors.Count);
        Assert.Contains(validation.Errors, x => x.Field is "Name");
        Assert.Contains(validation.Errors, x => x.Field is "Age");
    }

    [Fact]
    public void ValidationTest_WhenMultipleFieldsInvalid_AddsError()
    {
        var validation = new Validator<Person>(new Person("", 140))
            .RuleFor("Name", x => x.Name)
            .NotEmpty()
            .MinLength(10)
            .Equal("")
            .Matches(@"^b\d{8}")
            .Must(x => x.StartsWith('A'))
            .RuleFor("Age", x => x.Age)
            .LessThan(150)
            .Max(141)
            .Min(150)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.False(validation.IsValid);
        Assert.Equal(5, validation.Errors.Count);
        Assert.Contains(validation.Errors, x => x.Field is "Name");
        Assert.Contains(validation.Errors, x => x.Field is "Age");
    }

    [Theory]
    [InlineData("Alexander", 120)]
    [InlineData("Anastasia", 160)]
    public void ValidationTest_WhenAllFieldsValid_DoesNotAddError(string name, int age)
    {
        var validation = new Validator<Person>(new Person(name, age))
            .RuleFor("Name", x => x.Name)
            .NotEmpty()
            .MinLength(3)
            .Equal(name)
            .Must(x => x.StartsWith('A'))
            .RuleFor("Age", x => x.Age)
            .Max(160)
            .Validate();

        output.WriteLine(string.Join(Environment.NewLine, validation.Errors));

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }
}