using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Configuration;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Services;

internal sealed class OpenIdSettingsField
{
    public OpenIdSettingsField(string name, JsonObject schema, Func<JsonNode, bool> isValid, bool secret = false)
    {
        Name = name;
        Schema = schema;
        IsValid = isValid;
        Secret = secret;
    }

    public string Name { get; }
    public JsonObject Schema { get; }
    public Func<JsonNode, bool> IsValid { get; }
    public bool Secret { get; }

    public static OpenIdSettingsField Text(string name, bool secret = false) => new(name,
        new JsonObject { ["type"] = new JsonArray("string", "null"), ["maxLength"] = 4096, ["writeOnly"] = secret },
        node => node is null || node is JsonValue value && value.TryGetValue<string>(out var text) && text.Length <= 4096, secret);

    public static OpenIdSettingsField Address(string name) => new(name,
        new JsonObject { ["type"] = new JsonArray("string", "null"), ["format"] = "uri", ["maxLength"] = 4096 },
        node => node is null || node is JsonValue value && value.TryGetValue<string>(out var text)
            && text.Length <= 4096 && Uri.TryCreate(text, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp));

    public static OpenIdSettingsField Path(string name) => new(name,
        new JsonObject { ["type"] = new JsonArray("string", "null"), ["maxLength"] = 4096, ["pattern"] = "^(/[^?#\\r\\n]*)?$" },
        node => node is null || node is JsonValue value && value.TryGetValue<string>(out var text)
            && text.Length <= 4096 && (text.Length == 0 || text.StartsWith('/'))
            && !text.Contains('?') && !text.Contains('#') && !text.Contains('\r') && !text.Contains('\n'));

    public static OpenIdSettingsField Boolean(string name) => new(name,
        new JsonObject { ["type"] = "boolean" }, node => node is JsonValue value && value.TryGetValue<bool>(out _));

    public static OpenIdSettingsField Choice(string name, string[] choices, bool nullable = false) => new(name,
        new JsonObject { ["type"] = nullable ? new JsonArray("string", "null") : JsonValue.Create("string"),
            ["enum"] = new JsonArray(choices.Select(choice => (JsonNode)JsonValue.Create(choice)).Concat(nullable ? [null] : []).ToArray()), },
        node => nullable && node is null || node is JsonValue value && value.TryGetValue<string>(out var text) && choices.Contains(text, StringComparer.Ordinal));

    public static OpenIdSettingsField Names(string name) => new(name,
        new JsonObject { ["type"] = new JsonArray("array", "null"), ["maxItems"] = 128,
            ["items"] = new JsonObject { ["type"] = "string", ["maxLength"] = 256 }, },
        node => node is null || node is JsonArray array && array.Count <= 128 && array.All(item =>
            item is JsonValue value && value.TryGetValue<string>(out var text) && text.Length is > 0 and <= 256));

    public static OpenIdSettingsField Parameters() => new("parameters",
        new JsonObject { ["type"] = new JsonArray("array", "null"), ["maxItems"] = 128, ["writeOnly"] = true,
            ["items"] = new JsonObject { ["type"] = "object", ["additionalProperties"] = false,
                ["required"] = new JsonArray("name", "value"), ["properties"] = new JsonObject
                { ["name"] = new JsonObject { ["type"] = "string", ["maxLength"] = 256 },
                  ["value"] = new JsonObject { ["type"] = "string", ["maxLength"] = 4096 }, }, }, },
        node => node is null || node is JsonArray array && array.Count <= 128 && array.All(item =>
            item is JsonObject pair && pair.Count == 2 && pair["name"] is JsonValue name && name.TryGetValue<string>(out var key)
            && key.Length is > 0 and <= 256 && pair["value"] is JsonValue value && value.TryGetValue<string>(out var text) && text.Length <= 4096), true);
}

internal sealed class OpenIdSettingsSectionProvider<T> : ISiteSettingsSectionProvider where T : class
{
    private readonly Func<Task<T>> _get;
    private readonly Func<T, Task> _update;
    private readonly Func<T, Task<ImmutableArray<ValidationResult>>> _validate;
    private readonly IShellReleaseManager _release;
    private readonly OpenIdSettingsField[] _fields;
    private readonly IDataProtectionProvider _protection;

    public OpenIdSettingsSectionProvider(string name, string feature, Permission permission,
        OpenIdSettingsField[] fields, Func<Task<T>> get, Func<T, Task> update,
        Func<T, Task<ImmutableArray<ValidationResult>>> validate, IShellReleaseManager release,
        IDataProtectionProvider protection = null)
    {
        Descriptor = new() { Name = name, DisplayName = name, FeatureId = feature,
            Description = "Tenant OpenID settings. Omitted fields are preserved; updates validate with the existing OpenID service and reload the tenant. Secrets are write-only.",
            RequiresHttps = true, RequiresReload = true, };
        ReadPermission = permission;
        _fields = fields;
        _get = get;
        _update = update;
        _validate = validate;
        _release = release;
        _protection = protection;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; }
    public Permission ReadPermission { get; }
    public Permission UpdatePermission => ReadPermission;

    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject(_fields.Select(field => KeyValuePair.Create(field.Name, field.Schema.DeepClone()))),
    };

    public async Task<SiteSettingsSectionResponse> GetAsync() => Describe(Serialize(await _get()));

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = ["A settings object is required."] } };
        }
        foreach (var entry in values)
        {
            var field = _fields.FirstOrDefault(field => field.Name == entry.Key);
            if (field is null || !field.IsValid(entry.Value))
            {
                errors[entry.Key] = ["Provide a supported property value matching the section schema."];
            }
        }
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }

        var current = Serialize(await _get());
        var candidate = current.DeepClone().AsObject();
        foreach (var entry in values)
        {
            if (entry.Key == "clientSecret")
            {
                candidate[entry.Key] = OpenIdClientSecretEditor.Protect(_protection,
                    current[entry.Key]?.GetValue<string>(), entry.Value?.GetValue<string>());
            }
            else
            {
                candidate[entry.Key] = entry.Key == "parameters" && entry.Value is null ? new JsonArray() : entry.Value?.DeepClone();
            }
        }

        T settings;
        try
        {
            settings = candidate.Deserialize<T>(JOptions.CamelCase);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or FormatException)
        {
            return new() { Errors = new() { ["body"] = ["A setting has an invalid format."] } };
        }
        foreach (var validation in await _validate(settings))
        {
            foreach (var member in validation.MemberNames.DefaultIfEmpty("body"))
            {
                errors[JsonNamingPolicy.CamelCase.ConvertName(member)] = [validation.ErrorMessage];
            }
        }
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }

        var changed = !JsonNode.DeepEquals(current, Serialize(settings));
        if (changed)
        {
            await _update(settings);
            _release.RequestRelease();
        }
        return new() { Changed = changed, ReloadRequested = changed, Section = Describe(Serialize(settings)) };
    }

    private SiteSettingsSectionResponse Describe(JsonObject settings)
    {
        var values = new JsonObject();
        foreach (var field in _fields.Where(field => !field.Secret))
        {
            values[field.Name] = settings[field.Name]?.DeepClone();
        }
        if (_protection is not null)
        {
            values["hasClientSecret"] = !string.IsNullOrEmpty(settings["clientSecret"]?.GetValue<string>());
            values["hasParameters"] = settings["parameters"] is JsonArray { Count: > 0 };
        }
        return new() { Name = Descriptor.Name, Source = "tenant", Values = values,
            RedactedProperties = _fields.Where(field => field.Secret).Select(field => field.Name).ToArray(),
            ReadOnlyProperties = _protection is null ? [] : ["hasClientSecret", "hasParameters"], };
    }

    private static JsonObject Serialize(T settings) => JsonSerializer.SerializeToNode(settings, JOptions.CamelCase).AsObject();
}

internal static class OpenIdClientSecretEditor
{
    public static string Protect(IDataProtectionProvider provider, string current, string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return null;
        }
        var protector = provider.CreateProtector(nameof(OpenIdClientConfiguration));
        if (!string.IsNullOrEmpty(current))
        {
            try
            {
                if (protector.Unprotect(current) == plaintext)
                {
                    return current;
                }
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // A supplied replacement can recover a secret protected with unavailable keys.
            }
        }
        return protector.Protect(plaintext);
    }
}
