using System.Text.Json;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public sealed class CronExpressionControllerTests
{
    [Fact]
    public async Task Describe_ValidExpression_ReturnsItsDescriptionAndNextOccurrences()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        // Act
        using var utc = await GetAsync(context, "0 3 * * *", utc: true);
        using var site = await GetAsync(context, "0 3 * * *", utc: false);

        // Assert
        Assert.True(utc.RootElement.GetProperty("isValid").GetBoolean());
        Assert.False(string.IsNullOrEmpty(utc.RootElement.GetProperty("description").GetString()));
        Assert.Equal(3, utc.RootElement.GetProperty("occurrences").GetArrayLength());
        Assert.Equal("UTC", utc.RootElement.GetProperty("timeZone").GetString());

        // The time zone of the site is the default one, which the description doesn't tell.
        Assert.True(site.RootElement.GetProperty("isValid").GetBoolean());
        Assert.DoesNotContain("(", site.RootElement.GetProperty("description").GetString(), StringComparison.Ordinal);
        Assert.Equal(3, site.RootElement.GetProperty("occurrences").GetArrayLength());
        Assert.False(string.IsNullOrEmpty(site.RootElement.GetProperty("timeZone").GetString()));
    }

    [Theory]
    [InlineData("0 25 * * *")]
    [InlineData("0 0 3 * * *")]
    [InlineData("")]
    public async Task Describe_InvalidExpression_ReturnsAnError(string expression)
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        // Act
        using var result = await GetAsync(context, expression, utc: false);

        // Assert
        Assert.False(result.RootElement.GetProperty("isValid").GetBoolean());
        Assert.False(string.IsNullOrEmpty(result.RootElement.GetProperty("error").GetString()));
    }

    [Fact]
    public async Task Describe_ExpressionWithATimeZone_ReturnsItsOccurrencesInIt()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        // Act
        using var tokyo = await GetAsync(context, "0 3 * * *", utc: true, timeZone: "Asia/Tokyo");
        using var unknown = await GetAsync(context, "0 3 * * *", utc: true, timeZone: "Nowhere/Unknown");

        // Assert
        Assert.Equal("Asia/Tokyo", tokyo.RootElement.GetProperty("timeZone").GetString());
        Assert.EndsWith(" (JST)", tokyo.RootElement.GetProperty("description").GetString(), StringComparison.Ordinal);
        Assert.Equal(3, tokyo.RootElement.GetProperty("occurrences").GetArrayLength());

        // A time zone that is not known is ignored.
        Assert.Equal("UTC", unknown.RootElement.GetProperty("timeZone").GetString());
        Assert.EndsWith(" (UTC)", unknown.RootElement.GetProperty("description").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Describe_ExpressionThatNeverOccurs_ReturnsNoOccurrence()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        // Act
        using var result = await GetAsync(context, "0 0 31 2 *", utc: true);

        // Assert
        Assert.True(result.RootElement.GetProperty("isValid").GetBoolean());
        Assert.Equal(0, result.RootElement.GetProperty("occurrences").GetArrayLength());
    }

    private static async Task<JsonDocument> GetAsync(SiteContext context, string expression, bool utc, string timeZone = null)
    {
        var path = $"Admin/CronExpression/Describe?expression={Uri.EscapeDataString(expression)}&utc={(utc ? "true" : "false")}";

        if (timeZone is not null)
        {
            path += $"&timeZone={Uri.EscapeDataString(timeZone)}";
        }

        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode, $"{path} returned {(int)response.StatusCode}.");

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
