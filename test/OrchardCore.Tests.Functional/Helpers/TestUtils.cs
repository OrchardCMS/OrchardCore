namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardConfig
{
    public string Username { get; set; } = "admin";
    public string Email { get; set; } = "admin@orchard.com";
    public string Password { get; set; } = "Orchard1!";
}

public sealed class TenantInfo
{
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string SetupRecipe { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public static class TestUtils
{
    public static readonly OrchardConfig DefaultConfig = new();

    public static TenantInfo GenerateTenantInfo(string setupRecipeName, string description = "")
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var uniqueName = "t" + Convert.ToString((long)(now - today).TotalMilliseconds, 16);

        return new TenantInfo
        {
            Name = uniqueName,
            Prefix = uniqueName,
            SetupRecipe = setupRecipeName,
            Description = description,
        };
    }
}
