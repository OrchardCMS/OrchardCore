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
        var output = Redact<RemoveRedactor>(input);

        Assert.Null(output);
    }

    [Fact]
    public void RemoveRedactor_Random_Succeeds()
    {
        var output = Redact<RemoveRedactor>(Guid.NewGuid().ToString());

        Assert.Null(output);
    }

    [Theory]
    [InlineData("email", "e***l")]
    [InlineData("User Name", "U*******e")]
    [InlineData("Multiple line\ndata", "M****************a")]
    public void PartialAsteriskRedactor_Default_Succeeds(string input, string expectedOutput)
    {
        var output = Redact<PartialAsteriskRedactor>(input);

        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void PartialAsteriskRedactor_Random_Succeeds()
    {
        var input = Guid.NewGuid().ToString("B");
        var output = Redact<PartialAsteriskRedactor>(input);

        Assert.Equal("{************************************}", output);
    }

    private static string Redact<TRedactor>(string input)
        where TRedactor : Redactor, new()
    {
        var redactor = new TRedactor();
        return redactor.Redact(input);
    }
}
