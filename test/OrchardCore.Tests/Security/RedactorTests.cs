using Microsoft.Extensions.Compliance.Redaction;
using OrchardCore.Users.AuditTrail.Services;

namespace OrchardCore.Tests.Security;

public class RedactorTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("ab", "ab")]
    [InlineData("😀", "😀")]
    [InlineData("😀e\u0301", "😀e\u0301")]
    [InlineData("😀middle👩‍💻", "😀******👩‍💻")]
    [InlineData("ae\u0301b", "a*b")]
    [InlineData("a🇺🇸👍🏽b", "a**b")]
    [InlineData("e\u0301abc🇺🇸", "e\u0301***🇺🇸")]
    public void PartialAsteriskRedactor_TextElements_StringAndSpanAgree(string input, string expected)
    {
        var redactor = new PartialAsteriskRedactor();
        var length = redactor.GetRedactedLength(input.AsSpan());
        var destination = new char[length + 1];
        destination[^1] = '!';

        var written = redactor.Redact(input.AsSpan(), destination.AsSpan());

        Assert.Equal(expected, redactor.Redact(input));
        Assert.Equal(expected.Length, length);
        Assert.Equal(length, written);
        Assert.Equal(expected, new string(destination, 0, written));
        Assert.Equal('!', destination[^1]);
    }

    [Fact]
    public void PartialAsteriskRedactor_Null_ReturnsNull()
    {
        Assert.Null(new PartialAsteriskRedactor().Redact((string)null));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("😀middle👩‍💻")]
    public void PartialAsteriskRedactor_ShortDestination_ThrowsWithoutWriting(string input)
    {
        var redactor = new PartialAsteriskRedactor();
        var destination = Enumerable.Repeat('!', redactor.GetRedactedLength(input.AsSpan()) - 1).ToArray();

        Assert.Throws<ArgumentException>(() => redactor.Redact(input.AsSpan(), destination.AsSpan()));
        Assert.All(destination, character => Assert.Equal('!', character));
    }

    [Theory]
    [InlineData("email")]
    [InlineData("User Name")]
    [InlineData("Multiple line\ndata")]
    public void RemoveRedactor_Default_Succeeds(string input)
    {
        // Act
        var output = Redact<RemoveRedactor>(input);

        // Assert
        Assert.Null(output);
    }

    [Fact]
    public void RemoveRedactor_Random_Succeeds()
    {
        // Act
        var output = Redact<RemoveRedactor>(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(output);
    }

    [Theory]
    [InlineData("email", "e***l")]
    [InlineData("User Name", "U*******e")]
    [InlineData("Multiple line\ndata", "M****************a")]
    public void PartialAsteriskRedactor_Default_Succeeds(string input, string expectedOutput)
    {
       // Act
        var output = Redact<PartialAsteriskRedactor>(input);

        // Assert
        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void PartialAsteriskRedactor_Random_Succeeds()
    {
       // Arrange
        var input = Guid.NewGuid().ToString("B");
        
        // Act
        var output = Redact<PartialAsteriskRedactor>(input);

        // Assert
        Assert.Equal("{************************************}", output);
    }

    private static string Redact<TRedactor>(string input)
        where TRedactor : Redactor, new()
    {
        var redactor = new TRedactor();

        return redactor.Redact(input);
    }
}
