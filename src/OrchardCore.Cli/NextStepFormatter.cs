using System.Text.Json;

namespace OrchardCore.Cli;

internal static class NextStepFormatter
{
    public static string? Format(CommandOutput output)
    {
        var path = string.Join(' ', output.CommandPath);
        if (path is "install" or "tenants install" or "tenants enable-remote-management" &&
            Read(output.Json, "context") is { } provisionedContext && CanQuote(provisionedContext))
        {
            var readiness = path == "install" ? " Once the site is running," : string.Empty;
            return $"Next:{readiness} use the saved application credentials to explore this tenant.\n  {ContextCommand(provisionedContext, output.CurrentContextName)} --help";
        }

        var name = Read(output.Json, "name");
        if (path == "install" && Read(output.Json, "directory") is { } directory)
        {
            name = Path.GetFileName(Path.TrimEndingDirectorySeparator(directory));
        }
        var state = Read(output.Json, "state");
        var context = output.ContextName;
        if (context is not null && !CanQuote(context))
        {
            return null;
        }

        var parentCommand = ContextCommand(context, output.CurrentContextName);
        if (name is not null && CanQuote(name) && !name.StartsWith('-'))
        {
            var tenant = QuoteArgument(name);
            if (path == "tenants create" && state == "Uninitialized")
            {
                var command = $"{parentCommand} tenants setup {tenant} --site-name {tenant} --user-name admin --email {QuoteArgument("<admin-email>")}";
                var recipe = Read(output.Json, "recipeName");
                if (string.IsNullOrWhiteSpace(recipe))
                {
                    command += $" --recipe-name {QuoteArgument("<recipe-name>")}";
                }
                if (string.IsNullOrWhiteSpace(Read(output.Json, "databaseProvider")))
                {
                    command += " --database-provider Sqlite";
                }

                return $"Next: set up this tenant (replace the placeholders and adjust the site details).\n  {command}\nThe administrator password will be prompted securely. Choose a recipe such as SaaS or Blog if one is not already configured.";
            }

            if (path is "tenants create" or "tenants setup" or "tenants install" or "tenants start" && state == "Running")
            {
                return $"Next: enable remote management for this tenant.\n  {parentCommand} tenants enable-remote-management {tenant}";
            }

            if (path == "tenants create" && state == "Disabled")
            {
                return $"Next: start this tenant.\n  {parentCommand} tenants start {tenant}";
            }

            if (path == "install" || path == "tenants enable-remote-management" && state == "Running")
            {
                var url = GetTenantUrl(output.Json);
                var contextName = ChooseContextName(name, output.KnownContexts);
                var destination = url ?? "<tenant-url>";
                var note = url is null ? " Replace <tenant-url> with the exact tenant URL, including its path prefix." : string.Empty;
                var readiness = path == "install" ? " Once the site is running and Remote Management is configured," : string.Empty;
                return $"Next:{readiness} save a context with an unused name.{note} Contexts are shared across sessions; check the list and adjust the suggested name if needed.\n  pomi context list --output json\n  pomi context add {QuoteArgument(contextName)} {QuoteArgument(destination)} --current";
            }

            if (path == "context add")
            {
                return $"Next: sign in to this tenant.\n  {ContextCommand(name, output.CurrentContextName)} login";
            }
        }

        if (path == "login" && Read(output.Json, "context") is { } loggedInContext && CanQuote(loggedInContext))
        {
            return $"Next: explore the commands available for this tenant.\n  {ContextCommand(loggedInContext, output.CurrentContextName)} --help";
        }

        return null;
    }

    private static string ContextCommand(string? context, string? currentContext) => string.IsNullOrEmpty(context)
        || string.Equals(context, currentContext, StringComparison.OrdinalIgnoreCase)
        ? "pomi" : $"pomi --context={QuoteArgument(context)}";

    internal static string ChooseContextName(string name, IReadOnlyList<TenantContextRecord> contexts)
    {
        var candidate = name;
        var suffix = 2;
        while (contexts.Any(context => string.Equals(context.Name, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{name}-{suffix++}";
        }

        return candidate;
    }

    private static string? GetTenantUrl(JsonElement value)
    {
        var url = Read(value, "url") ?? Read(value, "primaryUrl");
        if (url is null || !CanQuote(url))
        {
            return null;
        }

        try
        {
            var uri = CliUriPolicy.RequireSecureEndpoint(url);
            return string.IsNullOrEmpty(uri.UserInfo + uri.Query + uri.Fragment) ? uri.AbsoluteUri : null;
        }
        catch (CliException)
        {
            return null;
        }
    }

    // Suggestions target POSIX shells on macOS/Linux and PowerShell on Windows.
    // Never allow server-provided names to become shell operators or expansions.
    internal static string QuoteArgument(string value, bool? powerShell = null)
    {
        if (value.Length > 0 && value.All(character => char.IsAsciiLetterOrDigit(character) || "-._/:@".Contains(character)))
        {
            return value;
        }

        return "'" + value.Replace("'", (powerShell ?? OperatingSystem.IsWindows()) ? "''" : "'\"'\"'", StringComparison.Ordinal) + "'";
    }

    private static bool CanQuote(string value) => !string.IsNullOrWhiteSpace(value) && !value.Any(char.IsControl);

    private static string? Read(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
}
