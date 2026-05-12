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
    public string TablePrefix { get; set; } = string.Empty;
}

public static class TestUtils
{
    public static readonly OrchardConfig DefaultConfig = new();

    public static TenantInfo GenerateTenantInfo(string setupRecipeName, string description = "")
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var ms = (long)(now - today).TotalMilliseconds;
        var uniqueName = "t" + ToBase32(ms);

        return new TenantInfo
        {
            Name = uniqueName,
            Prefix = uniqueName,
            SetupRecipe = setupRecipeName,
            Description = description,
            TablePrefix = uniqueName,
        };
    }

    private static string ToBase32(long value)
    {
        const string digits = "0123456789abcdefghijklmnopqrstuv";
        if (value == 0)
        {
            return "0";
        }

        var result = new char[13];
        var index = result.Length;
        while (value > 0)
        {
            result[--index] = digits[(int)(value % 32)];
            value /= 32;
        }

        return new string(result, index, result.Length - index);
    }
}
