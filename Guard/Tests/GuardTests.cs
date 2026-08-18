using GuardApp;

namespace Tests;

public class GuardTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CheckNullOrWhiteSpace_WhenNullOrWhiteSpace_Throws(string? value)
    {
        Assert.Throws<ArgumentException>(() => value!.CheckNullOrWhiteSpace());
    }

    [Fact]
    public void CheckNullOrWhiteSpace_WhenValid_ReturnsOriginalValue()
    {
        var result = "hello".CheckNullOrWhiteSpace();

        Assert.Equal("hello", result);
    }

    [Fact]
    public void CheckNullOrWhiteSpace_WhenTrimRequested_ReturnsTrimmedValue()
    {
        var result = "  hello  ".CheckNullOrWhiteSpace(trimValue: true);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void CheckNull_WhenNull_ThrowsArgumentNullException()
    {
        string? value = null;

        Assert.Throws<ArgumentNullException>(() => value.CheckNull());
    }

    [Fact]
    public void CheckNull_WhenPredicateFails_ThrowsArgumentException()
    {
        var value = "short";

        Assert.Throws<ArgumentException>(() =>
            value.CheckNull(predicate: s => s.Length > 10));
    }

    [Fact]
    public void CheckNull_WhenValidAndPredicatePasses_ReturnsValue()
    {
        var value = "long enough string";

        var result = value.CheckNull(predicate: s => s.Length > 10);

        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CheckNullOrEmpty_WhenNullOrEmpty_Throws(string? value)
    {
        Assert.Throws<ArgumentException>(() => value!.CheckNullOrEmpty());
    }

    [Fact]
    public void CheckNullOrEmpty_WhenWhitespaceOnly_DoesNotThrow()
    {
        var result = "   ".CheckNullOrEmpty();

        Assert.Equal("   ", result);
    }


    [Fact]
    public void CheckNotDefault_WhenDefault_Throws()
    {
        Assert.Throws<ArgumentException>(() => 0.CheckNotDefault());
    }

    [Fact]
    public void CheckNotDefault_WhenNotDefault_ReturnsValue()
    {
        var result = 5.CheckNotDefault();

        Assert.Equal(5, result);
    }


    [Fact]
    public void CheckNotEmpty_WhenNull_Throws()
    {
        List<int>? collection = null;

        Assert.Throws<ArgumentException>(() => collection.CheckNotEmpty());
    }

    [Fact]
    public void CheckNotEmpty_WhenEmpty_Throws()
    {
        var collection = new List<int>();

        Assert.Throws<ArgumentException>(() => collection.CheckNotEmpty());
    }

    [Fact]
    public void CheckNotEmpty_WhenNotEmpty_ReturnsCollection()
    {
        var collection = new List<int> { 1, 2, 3 };

        var result = collection.CheckNotEmpty();

        Assert.Equal(collection, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CheckTooLongOrEmpty_WhenNullOrWhiteSpace_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => value!.CheckTooLongOrEmpty(10));
    }

    [Fact]
    public void CheckTooLongOrEmpty_WhenTooLong_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => "this is too long".CheckTooLongOrEmpty(5));
    }

    [Fact]
    public void CheckTooLongOrEmpty_WhenValid_ReturnsValue()
    {
        var result = "short".CheckTooLongOrEmpty(10);

        Assert.Equal("short", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeValue_WhenNullOrWhiteSpace_ThrowsWithCorrectParamName(string? value)
    {
        var ex = Assert.Throws<ArgumentNullException>(() => value!.NormalizeValue());

        // Regression test for a constructor-overload bug: ArgumentNullException's
        // single-string constructor sets ParamName, not Message. ParamName must
        // be the actual parameter name, not the full sentence.
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void NormalizeValue_WhenValid_ReturnsTrimmedLowercasedValue()
    {
        var result = "  HELLO World  ".NormalizeValue();

        Assert.Equal("hello world", result);
    }


    [Fact]
    public void TrimValue_RemovesLeadingAndTrailingWhitespace()
    {
        var result = "  hello  ".TrimValue();

        Assert.Equal("hello", result);
    }


    [Fact]
    public void CheckStartHigherThanEnd_WhenStartAfterEnd_Throws()
    {
        var start = new DateTime(2026, 1, 10);
        var end = new DateTime(2026, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => start.CheckStartHigherThanEnd(end));
    }

    [Fact]
    public void CheckStartHigherThanEnd_WhenStartBeforeOrEqualEnd_DoesNotThrow()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 10);

        var exception = Record.Exception(() => start.CheckStartHigherThanEnd(end));

        Assert.Null(exception);
    }


    [Fact]
    public void CheckCreationDateTimeOffset_WhenInFuture_Throws()
    {
        var future = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => future.CheckCreationDateTimeOffset());
    }

    [Fact]
    public void CheckCreationDateTimeOffset_WhenPastOrPresent_DoesNotThrow()
    {
        var past = DateTimeOffset.UtcNow.AddDays(-1);

        var exception = Record.Exception(() => past.CheckCreationDateTimeOffset());

        Assert.Null(exception);
    }


    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@no-local-part.com")]
    public void ValidateEmailRegex_WhenInvalid_Throws(string email)
    {
        Assert.Throws<ArgumentException>(() => email.ValidateEmailRegex());
    }

    [Fact]
    public void ValidateEmailRegex_WhenValid_ReturnsValue()
    {
        var result = "john@example.com".ValidateEmailRegex();

        Assert.Equal("john@example.com", result);
    }

    [Fact]
    public void ValidateEmailRegex_WhenNormalizeRequested_ReturnsLowercasedTrimmedValue()
    {
        var result = "  JOHN@EXAMPLE.COM  ".ValidateEmailRegex(normalize: true);

        Assert.Equal("john@example.com", result);
    }


    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void ValidateEmailParsing_WhenInvalid_Throws(string email)
    {
        Assert.Throws<ArgumentException>(() => email.ValidateEmailParsing());
    }

    [Fact]
    public void ValidateEmailParsing_WhenValid_ReturnsValue()
    {
        var result = "john@example.com".ValidateEmailParsing();

        Assert.Equal("john@example.com", result);
    }


    [Fact]
    public void CheckIfZero_WhenZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => 0.CheckIfZero());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public void CheckIfZero_WhenNonZero_DoesNotThrow(int value)
    {
        var exception = Record.Exception(() => value.CheckIfZero());

        Assert.Null(exception);
    }
}