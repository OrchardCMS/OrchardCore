using Microsoft.Extensions.Compliance.Redaction;
using OrchardCore.Users.AuditTrail.Services;

namespace OrchardCore.Tests.Security;

public class RedactorTests
{
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
