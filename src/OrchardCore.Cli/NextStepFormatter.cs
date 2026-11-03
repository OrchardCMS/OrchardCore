using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal static class NextStepFormatter
{
    public static string? Format(CommandOutput output)
    {
        var path = string.Join(' ', output.CommandPath);
        if ((path == "install" || output.Operation?.ProvisioningProperty is not null) &&
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

        var context = output.ContextName;
        if (context is not null && !CanQuote(context))
        {
            return null;
        }

        if (output.Operation is { NextSteps.Count: > 0 } operation)
        {
            return FormatOperationStep(output, operation, name, ContextCommand(context, output.CurrentContextName));
        }

        if (IsSafeValue(name))
        {
            if (path == "install")
            {
                return FormatSaveContext(output, name, " Once the site is running and Remote Management is configured,");
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

    private static string? FormatOperationStep(CommandOutput output, CliOperationMetadata operation, string? name, string parentCommand)
    {
        var step = operation.NextSteps.FirstOrDefault(step =>
            string.IsNullOrEmpty(step.WhenProperty) || Read(output.Json, step.WhenProperty) == step.WhenValue);
        if (step is null)
        {
            return null;
        }

        if (step.SavesContext)
        {
            return IsSafeValue(name) ? FormatSaveContext(output, name, string.Empty) : null;
        }

        // This text comes from the tenant, and the output doesn't escape it.
        if (string.IsNullOrEmpty(step.Description) || !CanQuote(step.Description) || (!string.IsNullOrEmpty(step.Note) && !CanQuote(step.Note)))
        {
            return null;
        }

        var arguments = new List<string>();
        foreach (var argument in step.Arguments.Concat(step.OptionalArguments
            .Where(optional => string.IsNullOrWhiteSpace(Read(output.Json, optional.WhenMissing)))
            .SelectMany(optional => optional.Arguments)))
        {
            var isPlaceholder = argument.StartsWith('{') && argument.EndsWith('}');
            var value = isPlaceholder ? Read(output.Json, argument[1..^1]) : argument;
            if (value is null || !CanQuote(value) || (isPlaceholder && value.StartsWith('-')))
            {
                return null;
            }

            arguments.Add(QuoteArgument(value));
        }

        var note = string.IsNullOrEmpty(step.Note) ? string.Empty : $"\n{step.Note}";
        return $"Next: {step.Description}\n  {parentCommand} {string.Join(' ', arguments)}{note}";
    }

    private static string FormatSaveContext(CommandOutput output, string name, string readiness)
    {
        var url = GetTenantUrl(output.Json);
        var contextName = ChooseContextName(name, output.KnownContexts);
        var destination = url ?? "<tenant-url>";
        var note = url is null ? " Replace <tenant-url> with the exact tenant URL, including its path prefix." : string.Empty;
        return $"Next:{readiness} save a context with an unused name.{note} Contexts are shared across sessions; check the list and adjust the suggested name if needed.\n  pomi context list --output json\n  pomi context add {QuoteArgument(contextName)} {QuoteArgument(destination)} --current";
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

    private static bool IsSafeValue([NotNullWhen(true)] string? value) => value is not null && CanQuote(value) && !value.StartsWith('-');

    private static bool CanQuote(string value) => !string.IsNullOrWhiteSpace(value) && !value.Any(char.IsControl);

    private static string? Read(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
}
