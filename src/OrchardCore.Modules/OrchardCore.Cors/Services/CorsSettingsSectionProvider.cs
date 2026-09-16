using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OrchardCore.Cors.Settings;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Cors.Services;

internal sealed class CorsSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private static readonly string[] s_flags = ["allowAnyOrigin", "allowAnyMethod", "allowAnyHeader", "allowCredentials", "isDefaultPolicy"];
    private static readonly string[] s_lists = ["allowedOrigins", "allowedMethods", "allowedHeaders", "exposedHeaders"];
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
    private readonly CorsService _service;
    private readonly IShellReleaseManager _releaseManager;

    public CorsSettingsSectionProvider(CorsService service, IShellReleaseManager releaseManager)
    {
        _service = service;
        _releaseManager = releaseManager;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "cors", DisplayName = "CORS", FeatureId = "OrchardCore.Cors",
        Description = "Tenant CORS policies. Omitted policies are preserved; a supplied array fully replaces the policies, including empty-array deletion.",
        RequiresReload = true,
    };
    public Permission ReadPermission => Permissions.ManageCorsSettings;
    public Permission UpdatePermission => Permissions.ManageCorsSettings;

    public JsonObject GetSchema()
    {
        var properties = new JsonObject { ["name"] = new JsonObject { ["type"] = "string", ["minLength"] = 1, ["maxLength"] = 256 } };
        foreach (var flag in s_flags)
        {
            properties[flag] = new JsonObject { ["type"] = "boolean", ["default"] = false };
        }
        foreach (var list in s_lists)
        {
            properties[list] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "string" }, ["default"] = new JsonArray() };
        }
        properties["allowedOrigins"]["description"] = "HTTP(S) origins without paths/trailing slashes, or '*' without credentials.";
        properties["isDefaultPolicy"]["description"] = "At most one policy may be default; otherwise the first policy is used.";
        return new()
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema", ["title"] = "CORS settings update",
            ["type"] = "object", ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["policies"] = new JsonObject
                {
                    ["type"] = "array", ["description"] = "Complete replacement; omit to preserve policies, or use [] to remove them. Null is not supported.",
                    ["items"] = new JsonObject { ["type"] = "object", ["additionalProperties"] = false, ["required"] = new JsonArray("name"), ["properties"] = properties },
                },
            },
        };
    }

    public async Task<SiteSettingsSectionResponse> GetAsync() => ToResponse(await _service.GetSettingsAsync());

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null || values.Any(entry => entry.Key != "policies"))
        {
            return Invalid("body", "Provide an object containing only the optional policies array.");
        }
        if (!values.TryGetPropertyValue("policies", out var node))
        {
            return new() { Section = await GetAsync() };
        }
        if (node is not JsonArray policies)
        {
            return Invalid("policies", "Provide an array of policy objects; null is not a reset operation.");
        }
        var settings = new List<CorsPolicySetting>();
        for (var index = 0; index < policies.Count; index++)
        {
            if (policies[index] is not JsonObject policy || policy.Any(entry => entry.Key != "name" && !s_flags.Contains(entry.Key, StringComparer.Ordinal) && !s_lists.Contains(entry.Key, StringComparer.Ordinal)))
            {
                return Invalid($"policies[{index}]", "Provide a policy object with only the documented properties.");
            }
            foreach (var list in s_lists)
            {
                if (policy.TryGetPropertyValue(list, out var listNode) && listNode is not JsonArray)
                {
                    return Invalid($"policies[{index}].{list}", "Provide a string array, not null.");
                }
            }
            try
            {
                settings.Add(policy.Deserialize<CorsPolicySetting>(s_jsonOptions));
            }
            catch (JsonException)
            {
                return Invalid($"policies[{index}]", "Policy properties must match their string, Boolean or string-array schema types.");
            }
        }
        var result = await _service.UpdateSettingsAsync(new CorsSettings { Policies = settings });
        if (result.Errors.Count > 0)
        {
            return new() { Errors = result.Errors };
        }
        if (result.Changed)
        {
            _releaseManager.RequestRelease();
        }
        return new() { Section = ToResponse(result.Settings), Changed = result.Changed, ReloadRequested = result.Changed };
    }

    private static SiteSettingsSectionUpdateResult Invalid(string key, string message) => new() { Errors = new() { [key] = [message] } };

    private static SiteSettingsSectionResponse ToResponse(CorsSettings settings) => new()
    {
        Name = "cors", Source = "tenant",
        Values = new JsonObject
        {
            ["policies"] = new JsonArray((settings.Policies ?? []).Select(policy => policy is null ? null : new JsonObject
            {
                ["name"] = policy.Name, ["isDefaultPolicy"] = policy.IsDefaultPolicy,
                ["allowAnyOrigin"] = policy.AllowAnyOrigin, ["allowCredentials"] = policy.AllowCredentials,
                ["allowAnyMethod"] = policy.AllowAnyMethod, ["allowAnyHeader"] = policy.AllowAnyHeader,
                ["allowedOrigins"] = Strings(policy.AllowedOrigins), ["allowedMethods"] = Strings(policy.AllowedMethods),
                ["allowedHeaders"] = Strings(policy.AllowedHeaders), ["exposedHeaders"] = Strings(policy.ExposedHeaders),
            }).ToArray()),
        },
    };

    private static JsonArray Strings(string[] values) => new((values ?? []).Select(value => (JsonNode)JsonValue.Create(value)).ToArray());
}
