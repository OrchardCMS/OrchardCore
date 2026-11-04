using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Users.Services.Management;

internal sealed class UserPolicyField<TSettings>
{
    public string Name { get; init; }
    public JsonObject Schema { get; init; }
    public Func<TSettings, JsonNode> Read { get; init; }
    public Func<TSettings, JsonNode, bool> Write { get; init; }

    public static UserPolicyField<TSettings> Boolean(string name, Func<TSettings, bool> read, Action<TSettings, bool> write) => new()
    {
        Name = name, Schema = new JsonObject { ["type"] = "boolean" }, Read = settings => JsonValue.Create(read(settings)),
        Write = (settings, node) =>
        {
            if (node is not JsonValue value || !value.TryGetValue<bool>(out var enabled)) { return false; }
            write(settings, enabled);
            return true;
        },
    };

    public static UserPolicyField<TSettings> Names(string name, Func<TSettings, string[]> read, Action<TSettings, string[]> write) => new()
    {
        Name = name,
        Schema = new JsonObject
        {
            ["type"] = new JsonArray("array", "null"), ["maxItems"] = 1024,
            ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1, ["maxLength"] = 256 },
        },
        Read = settings => read(settings) is { } names ? new JsonArray(names.Select(name => (JsonNode)JsonValue.Create(name)).ToArray()) : null,
        Write = (settings, node) =>
        {
            if (node is null) { write(settings, null); return true; }
            if (node is not JsonArray values || values.Count > 1024) { return false; }
            var names = new List<string>();
            foreach (var item in values)
            {
                if (item is not JsonValue value || !value.TryGetValue<string>(out var name) || string.IsNullOrWhiteSpace(name) || name.Length > 256) { return false; }
                names.Add(name);
            }
            write(settings, names.ToArray());
            return true;
        },
    };

    public static UserPolicyField<TSettings> Text(string name, Func<TSettings, string> read, Action<TSettings, string> write) => new()
    {
        Name = name, Schema = new JsonObject { ["type"] = new JsonArray("string", "null"), ["maxLength"] = 65536 },
        Read = settings => JsonValue.Create(read(settings)),
        Write = (settings, node) =>
        {
            if (node is null) { write(settings, null); return true; }
            if (node is not JsonValue value || !value.TryGetValue<string>(out var text) || text.Length > 65536) { return false; }
            write(settings, text);
            return true;
        },
    };

    public static UserPolicyField<TSettings> Integer(string name, int minimum, int maximum, Func<TSettings, int> read, Action<TSettings, int> write) => new()
    {
        Name = name,
        Schema = new JsonObject { ["type"] = "integer", ["minimum"] = minimum, ["maximum"] = maximum },
        Read = settings => JsonValue.Create(read(settings)),
        Write = (settings, node) =>
        {
            if (node is not JsonValue value || !value.TryGetValue<int>(out var number) || number < minimum || number > maximum) { return false; }
            write(settings, number);
            return true;
        },
    };
}

internal sealed class UserPolicySectionProvider<TSettings> : ISiteSettingsSectionProvider where TSettings : class, new()
{
    private readonly ISiteService _siteService;
    private readonly IReadOnlyList<UserPolicyField<TSettings>> _fields;
    private readonly Func<TSettings, TSettings> _clone;
    private readonly Func<TSettings, TSettings, bool> _apply;
    private readonly Action _changed;
    private readonly Func<TSettings, Task<Dictionary<string, string[]>>> _validate;

    public UserPolicySectionProvider(ISiteService siteService, string name, string feature,
        IReadOnlyList<UserPolicyField<TSettings>> fields, Func<TSettings, TSettings> clone,
        Func<TSettings, TSettings, bool> apply, Action changed = null, Func<TSettings, Task<Dictionary<string, string[]>>> validate = null)
    {
        _siteService = siteService;
        _fields = fields;
        _clone = clone;
        _apply = apply;
        _changed = changed;
        _validate = validate;
        Descriptor = new()
        {
            Name = name, DisplayName = name, FeatureId = feature, RequiresHttps = true,
            Description = "Tenant user policy. Omitted fields retain their values; nullable text can be cleared with null. Does not expose user credentials, recovery codes or authentication keys.",
        };
    }

    public SiteSettingsSectionDescriptor Descriptor { get; }
    public Permission ReadPermission => UsersPermissions.ManageUsers;
    public Permission UpdatePermission => UsersPermissions.ManageUsers;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema", ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject(_fields.Select(field => KeyValuePair.Create<string, JsonNode>(field.Name, field.Schema.DeepClone()))),
    };

    public async Task<SiteSettingsSectionResponse> GetAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync();
        return Describe(site.GetOrCreate<TSettings>());
    }

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = ["A policy object is required."] } };
        }
        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<TSettings>();
        var proposed = _clone(current);
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in values)
        {
            var field = _fields.FirstOrDefault(field => field.Name == entry.Key);
            if (field is null)
            {
                errors[entry.Key] = ["This property is not managed by this user policy section."];
            }
            else if (!field.Write(proposed, entry.Value))
            {
                errors[entry.Key] = ["Provide a value of the documented type and range; null is not supported."];
            }
        }
        foreach (var field in _fields)
        {
            if (!field.Write(proposed, field.Read(proposed)))
            {
                errors[field.Name] = ["The resulting policy is outside the supported field range."];
            }
        }
        if (_validate is not null && errors.Count == 0)
        {
            foreach (var error in await _validate(proposed)) { errors[char.ToLowerInvariant(error.Key[0]) + error.Key[1..]] = error.Value; }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }
        var changed = _apply(current, proposed);
        if (changed)
        {
            site.Put(current);
            await _siteService.UpdateSiteSettingsAsync(site);
            _changed?.Invoke();
        }
        return new() { Changed = changed, Section = Describe(current) };
    }

    private SiteSettingsSectionResponse Describe(TSettings settings) => new()
    {
        Name = Descriptor.Name, Source = "tenant",
        Values = new JsonObject(_fields.Select(field => KeyValuePair.Create(field.Name, field.Read(settings)))),
    };
}
