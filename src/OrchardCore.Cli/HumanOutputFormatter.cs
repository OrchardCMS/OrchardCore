using System.Text;
using System.Text.Json;

namespace OrchardCore.Cli;

internal static class HumanOutputFormatter
{
    public static string Format(CommandOutput output, bool useColor = false)
    {
        var verb = output.CommandPath.Count > 0 ? output.CommandPath[^1] : string.Empty;
        if (verb == "schema")
        {
            return OutputFormatter.FormatJson(output.Json);
        }

        var mutation = output.HttpMethod is not null
            ? output.HttpMethod is not ("GET" or "HEAD")
            : PastTense(verb) is not null;
        if (!mutation && (output.Json.ValueKind == JsonValueKind.Array || verb is "list" or "search"))
        {
            return OutputFormatter.FormatTable(output.Json, output.TableColumns);
        }

        var builder = new StringBuilder();
        var summary = GetSummary(output, mutation);
        if (summary is not null)
        {
            builder.AppendLine(Safe(summary)).AppendLine();
        }

        var path = string.Join(' ', output.CommandPath);
        IReadOnlySet<string>? fields = ReportsFailure(output.Json) || output.StatusCode == 202 ? null : path == "install"
            ? new HashSet<string>(["directory", "url", "tenant"])
            : mutation && output.CommandPath.Count == 2 && output.CommandPath[0] == "tenants"
                ? new HashSet<string>(["name", "tenantId", "state", "setupUrl", "primaryUrl", "url", "requestUrlHost", "requestUrlPrefix", "recipeName", "databaseProvider"])
                : null;
        AppendValue(builder, output.Json, 0, skipTransportFields: output.HttpMethod is not null, fields);
        if (path == "install" && Property(output.Json, "listenUrl") is { ValueKind: JsonValueKind.String } listenUrl)
        {
            AppendHint(builder, $"To start the site later, run from that directory:\n  dotnet run --no-launch-profile --urls \"{Safe(listenUrl.GetString()!)}\"", useColor);
        }
        if (!ReportsFailure(output.Json) && output.StatusCode != 202 && NextStepFormatter.Format(output) is { } nextStep)
        {
            AppendHint(builder, nextStep, useColor);
        }
        return builder.Length == 0 ? "No results." : builder.ToString().TrimEnd();
    }

    private static void AppendHint(StringBuilder builder, string hint, bool useColor)
    {
        builder.AppendLine();
        if (useColor)
        {
            builder.Append("\u001b[36m"); // Cyan distinguishes guidance from success and warning messages.
        }

        builder.Append(hint);
        if (useColor)
        {
            builder.Append("\u001b[0m");
        }

        builder.AppendLine();
    }

    private static string? GetSummary(CommandOutput output, bool mutation)
    {
        if (output.StatusCode == 202)
        {
            return "Request accepted. The operation may still be in progress.";
        }

        if (ReportsFailure(output.Json))
        {
            return "The operation reported an unsuccessful result. Review the details below.";
        }

        var path = string.Join(' ', output.CommandPath);
        var known = path switch
        {
            "login" => "Authenticated successfully.",
            "login device start" or "login device show" => "Device authorization is awaiting approval.",
            "login device wait" => "Authenticated successfully.",
            "logout" => Property(output.Json, "removed").ValueKind == JsonValueKind.False
                ? "No stored login was found for this context." : "Logged out successfully.",
            "context add" => "Context saved successfully.",
            "context use" => "Current context updated successfully.",
            "context remove" => "Context removed successfully.",
            "context clear" => Property(output.Json, "cleared").ValueKind == JsonValueKind.False
                ? "Cancelled. No contexts were removed." : "Contexts cleared successfully.",
            "api refresh" => "API metadata is up to date.",
            "docs refresh" => "Documentation index is up to date.",
            "install" => "Site created and initialized successfully.",
            "tenants install" => "Tenant created and initialized successfully.",
            _ => null,
        };
        if (known is not null || !mutation)
        {
            return known;
        }

        var verb = output.CommandPath.Count > 0 ? output.CommandPath[^1] : string.Empty;
        var group = string.Join(' ', output.CommandPath.SkipLast(1));
        var resource = group switch
        {
            "tenants" => "Tenant",
            "content items" => "Content item",
            "content types" => "Content type",
            "media files" => "Media file",
            "media folders" => "Media folder",
            "users" => "User",
            "roles" => "Role",
            _ => null,
        };
        // Create may return an existing resource with 200 (idempotent APIs).
        // Only a 201 response warrants claiming that a new resource was created.
        if (resource is not null && PastTense(verb) is { } action && (verb != "create" || output.StatusCode == 201))
        {
            var name = Property(output.Json, "name");
            var identity = name.ValueKind == JsonValueKind.String ? $" '{name.GetString()}'" : string.Empty;
            return $"{resource}{identity} {action} successfully.";
        }

        return path.Length == 0 ? "Operation completed successfully." : $"'{path}' completed successfully.";
    }

    private static string? PastTense(string verb) => verb switch
    {
        "create" => "created", "update" or "patch" => "updated", "delete" or "remove" => "removed",
        "enable" => "enabled", "disable" => "disabled", "upload" => "uploaded", "publish" => "published",
        "unpublish" => "unpublished", "import" => "imported", "export" => "exported", "execute" => "executed",
        "start" => "started", "stop" => "stopped", "clone" => "cloned", "move" => "moved", "rename" => "renamed",
        "restore" => "restored", "reset" => "reset", "save" => "saved", "setup" => "set up",
        "assign" => "assigned", "revoke" => "revoked", "apply" => "applied", "rebuild" => "rebuilt",
        "enable-remote-management" => "configured for remote management",
        "add" => "added", "clear" => "cleared", "refresh" => "refreshed", "install" => "installed",
        _ => null,
    };

    private static void AppendValue(StringBuilder builder, JsonElement value, int indent, bool skipTransportFields = false, IReadOnlySet<string>? fields = null)
    {
        var prefix = new string(' ', indent);
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in value.EnumerateObject().OrderBy(property => Priority(property.Name)))
                {
                    if (fields is not null && !fields.Contains(property.Name)
                        || property.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                        || property.Value.ValueKind == JsonValueKind.String && property.Value.GetString()!.Length == 0
                        || skipTransportFields && property.Name is "statusCode" or "contentType")
                    {
                        continue;
                    }

                    builder.Append(prefix).Append(Label(property.Name)).Append(':');
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        builder.AppendLine();
                        AppendValue(builder, property.Value, indent + 2);
                    }
                    else
                    {
                        builder.Append(' ').AppendLine(Scalar(property.Value));
                    }
                }
                break;
            case JsonValueKind.Array:
                if (value.GetArrayLength() == 0)
                {
                    builder.Append(prefix).AppendLine("None.");
                }
                foreach (var item in value.EnumerateArray())
                {
                    if (item.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        builder.Append(prefix).AppendLine("-");
                        AppendValue(builder, item, indent + 2);
                    }
                    else
                    {
                        builder.Append(prefix).Append("- ").AppendLine(Scalar(item));
                    }
                }
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                break;
            default:
                builder.Append(prefix).AppendLine(Scalar(value));
                break;
        }
    }

    private static int Priority(string name) => name switch
    {
        "setupUrl" => 0,
        "url" or "tenantUrl" or "siteUrl" or "adminUrl" or "primaryUrl" => 1,
        "name" or "displayName" or "id" or "tenantId" or "contentItemId" or "directory" or "path" => 2,
        "state" or "status" or "tenantState" => 3,
        _ => 4,
    };

    private static string Label(string name)
    {
        var words = CliUtilities.ToCliName(name).Split('-').Select(word => word switch
        {
            "url" => "URL", "id" => "ID", "sdk" => "SDK", "cli" => "CLI", "api" => "API",
            _ => word,
        });
        var label = Safe(string.Join(' ', words));
        return label.Length == 0 ? label : char.ToUpperInvariant(label[0]) + label[1..];
    }

    private static string Scalar(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.True => "Yes", JsonValueKind.False => "No", JsonValueKind.Null => "None",
        _ => Safe(value.ToString()),
    };

    private static JsonElement Property(JsonElement value, string name) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out var property) ? property : default;

    private static bool ReportsFailure(JsonElement value) =>
        new[] { "success", "succeeded", "successful", "valid", "isValid" }.Any(name => Property(value, name).ValueKind == JsonValueKind.False)
        || Property(value, "errors") is { ValueKind: JsonValueKind.Array } errors && errors.GetArrayLength() > 0
        || Property(value, "errors") is { ValueKind: JsonValueKind.Object } validationErrors && validationErrors.EnumerateObject().Any();

    private static string Safe(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value)
        {
            builder.Append(char.IsControl(character) ? $"\\u{(int)character:x4}" : character.ToString());
        }
        return builder.ToString();
    }
}
