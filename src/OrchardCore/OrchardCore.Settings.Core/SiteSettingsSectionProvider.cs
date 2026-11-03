using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

/// <summary>
/// A settings section backed by one settings type stored on the site. The properties declared by
/// <see cref="Describe"/> are the allowlist of the section: its schema, its readback and the values it accepts all
/// come from that single declaration, so that a property can't be exposed by one and forgotten by another.
/// </summary>
/// <typeparam name="TSettings">The settings type stored on the site.</typeparam>
public abstract class SiteSettingsSectionProvider<TSettings> : ISiteSettingsSectionProvider
    where TSettings : class, new()
{
    private readonly ISiteService _siteService;
    private readonly IShellReleaseManager _shellReleaseManager;
    private SiteSettingsSectionProperties<TSettings> _properties;

    protected SiteSettingsSectionProvider(ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
    {
        _siteService = siteService;
        _shellReleaseManager = shellReleaseManager;
        S = localizer;
    }

    /// <summary>Gets the localizer of the validation messages.</summary>
    protected IStringLocalizer S { get; }

    /// <inheritdoc/>
    public abstract SiteSettingsSectionDescriptor Descriptor { get; }

    /// <summary>Gets the permission the admin page of these settings requires, used for both reads and updates.</summary>
    protected abstract Permission ManagePermission { get; }

    /// <inheritdoc/>
    public Permission ReadPermission => ManagePermission;

    /// <inheritdoc/>
    public Permission UpdatePermission => ManagePermission;

    private SiteSettingsSectionProperties<TSettings> Properties
    {
        get
        {
            if (_properties is null)
            {
                _properties = new SiteSettingsSectionProperties<TSettings>(S);
                Describe(_properties);
            }

            return _properties;
        }
    }

    /// <summary>Declares the properties of the section, with the validation of the admin editor.</summary>
    /// <param name="properties">The properties of the section.</param>
    protected abstract void Describe(SiteSettingsSectionProperties<TSettings> properties);

    /// <summary>Gets why configuration owns the section, or <see langword="null"/> when the tenant does.</summary>
    protected virtual string GetReadOnlyReason() => null;

    /// <summary>Gets the settings to read back, which differ from the stored ones when configuration owns them.</summary>
    /// <param name="stored">The settings stored on the site.</param>
    protected virtual TSettings GetEffectiveSettings(TSettings stored) => stored;

    /// <summary>Validates what depends on several properties or on services.</summary>
    /// <param name="values">The supplied values.</param>
    /// <param name="proposed">The settings with the supplied values applied.</param>
    /// <param name="errors">The errors, by property name.</param>
    protected virtual Task ValidateAsync(JsonObject values, TSettings proposed, Dictionary<string, string[]> errors) => Task.CompletedTask;

    /// <summary>Applies the consequences of a valid change, before the site is saved.</summary>
    /// <param name="site">The site being saved.</param>
    /// <param name="current">The settings before the change.</param>
    /// <param name="proposed">The settings after the change.</param>
    protected virtual void OnChanging(ISite site, TSettings current, TSettings proposed)
    {
    }

    /// <inheritdoc/>
    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["title"] = $"{Descriptor.DisplayName} settings update",
        ["description"] = "All fields are optional; omitted fields retain their values.",
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = Properties.GetSchema(),
    };

    /// <inheritdoc/>
    public async Task<SiteSettingsSectionResponse> GetAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync();

        return ToResponse(GetEffectiveSettings(site.GetOrCreate<TSettings>()));
    }

    /// <inheritdoc/>
    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = [S["A settings object is required."]] } };
        }

        if (values.Count == 0)
        {
            return new() { Section = await GetAsync() };
        }

        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<TSettings>();
        var proposed = site.GetOrCreate<TSettings>();
        var errors = new Dictionary<string, string[]>();

        Properties.Apply(values, proposed, errors);
        await ValidateAsync(values, proposed, errors);

        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }

        var changed = !JsonNode.DeepEquals(JObject.FromObject(current), JObject.FromObject(proposed));

        if (changed)
        {
            OnChanging(site, current, proposed);
            site.Put(proposed);
            await _siteService.UpdateSiteSettingsAsync(site);

            if (Descriptor.RequiresReload)
            {
                _shellReleaseManager.RequestRelease();
            }
        }

        return new() { Section = ToResponse(proposed), Changed = changed, ReloadRequested = changed && Descriptor.RequiresReload };
    }

    private SiteSettingsSectionResponse ToResponse(TSettings settings)
    {
        var readOnlyReason = GetReadOnlyReason();

        return new()
        {
            Name = Descriptor.Name,
            Source = readOnlyReason is null ? "tenant" : "configuration",
            IsReadOnly = readOnlyReason is not null,
            ReadOnlyReason = readOnlyReason,
            Values = Properties.GetValues(settings),
            RedactedProperties = Properties.SecretNames,
        };
    }
}

/// <summary>The properties of a <see cref="SiteSettingsSectionProvider{TSettings}"/>.</summary>
/// <typeparam name="TSettings">The settings type stored on the site.</typeparam>
public sealed class SiteSettingsSectionProperties<TSettings>
{
    // The format the admin editors require from an authentication callback path.
    private const string CallbackPathPattern = @"\/[-A-Za-z0-9+&@#\/%?=~_|!:,.;]+[-A-Za-z0-9+&@#\/%=~_|]";

    private readonly IStringLocalizer S;
    private readonly JsonObject _schema = [];
    private readonly Dictionary<string, Func<JsonNode, TSettings, bool>> _readers = new(StringComparer.Ordinal);
    private readonly List<Action<TSettings, JsonObject>> _writers = [];
    private readonly List<Action<JsonObject, TSettings, Dictionary<string, string[]>>> _validators = [];
    private readonly List<string> _secretNames = [];

    internal SiteSettingsSectionProperties(IStringLocalizer localizer)
    {
        S = localizer;
    }

    internal string[] SecretNames => [.. _secretNames];

    /// <summary>Adds a Boolean property.</summary>
    public void Boolean(string name, string description, Func<TSettings, bool> get, Action<TSettings, bool> set)
    {
        _schema[name] = new JsonObject { ["type"] = "boolean", ["description"] = description };
        _writers.Add((settings, values) => values[name] = get(settings));
        _readers[name] = (node, settings) =>
        {
            if (node is JsonValue value && value.TryGetValue<bool>(out var parsed))
            {
                set(settings, parsed);

                return true;
            }

            return false;
        };
    }

    /// <summary>Adds a text property. As in the admin editors, an empty value is stored as <see langword="null"/>.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The schema description.</param>
    /// <param name="get">Reads the value from the settings.</param>
    /// <param name="set">Writes the value to the settings.</param>
    /// <param name="required">Whether the settings are invalid without a value, including when it isn't supplied.</param>
    /// <param name="pattern">A regular expression a value has to match entirely.</param>
    public void String(string name, string description, Func<TSettings, string> get, Action<TSettings, string> set, bool required = false, string pattern = null)
    {
        var schema = new JsonObject { ["description"] = description };
        if (required)
        {
            schema["type"] = "string";
            schema["minLength"] = 1;
        }
        else
        {
            schema["type"] = new JsonArray("string", "null");
        }

        if (pattern is not null)
        {
            schema["pattern"] = $"^(?:{pattern})$";
        }

        _schema[name] = schema;
        _writers.Add((settings, values) => values[name] = get(settings));
        _readers[name] = (node, settings) =>
        {
            if (node is null)
            {
                set(settings, null);

                return true;
            }

            // The format is checked before the value is applied, as the settings may not be able to hold an invalid one.
            if (node is JsonValue value && value.TryGetValue<string>(out var parsed)
                && (pattern is null || new RegularExpressionAttribute(pattern).IsValid(parsed)))
            {
                set(settings, string.IsNullOrEmpty(parsed) ? null : parsed);

                return true;
            }

            return false;
        };

        if (required)
        {
            _validators.Add((_, settings, errors) =>
            {
                if (string.IsNullOrWhiteSpace(get(settings)))
                {
                    errors[name] = [S["The {0} property is required.", name]];
                }
            });
        }
    }

    /// <summary>Adds an authentication callback path, in the format of the admin editors.</summary>
    public void CallbackPath(string name, string description, Func<TSettings, PathString> get, Action<TSettings, PathString> set)
        => String(
            name,
            description,
            settings => get(settings).HasValue ? get(settings).Value : null,
            (settings, value) => set(settings, string.IsNullOrEmpty(value) ? PathString.Empty : new PathString(value)),
            pattern: CallbackPathPattern);

    /// <summary>Adds an enumeration property, whose values are its member names.</summary>
    public void Enum<TEnum>(string name, string description, Func<TSettings, TEnum> get, Action<TSettings, TEnum> set)
        where TEnum : struct, Enum
    {
        _schema[name] = new JsonObject
        {
            ["type"] = "string",
            ["enum"] = new JsonArray([.. System.Enum.GetNames<TEnum>().Select(value => JsonValue.Create(value))]),
            ["description"] = description,
        };
        _writers.Add((settings, values) => values[name] = get(settings).ToString());
        _readers[name] = (node, settings) =>
        {
            if (TryParseEnum<TEnum>(node, out var parsed))
            {
                set(settings, parsed);

                return true;
            }

            return false;
        };
    }

    /// <summary>Adds an enumeration property that null resets.</summary>
    public void NullableEnum<TEnum>(string name, string description, Func<TSettings, TEnum?> get, Action<TSettings, TEnum?> set)
        where TEnum : struct, Enum
    {
        var values = new JsonArray([.. System.Enum.GetNames<TEnum>().Select(value => JsonValue.Create(value))]);
        values.Add(null);
        _schema[name] = new JsonObject
        {
            ["type"] = new JsonArray("string", "null"),
            ["enum"] = values,
            ["description"] = description,
        };
        _writers.Add((settings, target) => target[name] = get(settings)?.ToString());
        _readers[name] = (node, settings) =>
        {
            if (node is null)
            {
                set(settings, null);

                return true;
            }

            if (TryParseEnum<TEnum>(node, out var parsed))
            {
                set(settings, parsed);

                return true;
            }

            return false;
        };
    }

    /// <summary>
    /// Adds a secret: <c>{name}</c> sets it and is never returned, <c>clear{Name}</c> removes it, and
    /// <c>has{Name}</c> reads back whether there is one.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The schema description.</param>
    /// <param name="hasValue">Whether the settings hold a value.</param>
    /// <param name="set">Stores a new value.</param>
    /// <param name="clear">Removes the value.</param>
    public void Secret(string name, string description, Func<TSettings, bool> hasValue, Action<TSettings, string> set, Action<TSettings> clear)
    {
        var suffix = char.ToUpperInvariant(name[0]) + name[1..];
        var clearName = "clear" + suffix;

        _secretNames.Add(name);
        _schema[name] = new JsonObject { ["type"] = "string", ["writeOnly"] = true, ["minLength"] = 1, ["description"] = description };
        _schema[clearName] = new JsonObject { ["type"] = "boolean", ["description"] = $"Removes the stored value of {name}." };
        _writers.Add((settings, values) => values["has" + suffix] = hasValue(settings));
        _readers[name] = (node, settings) =>
        {
            if (node is JsonValue value && value.TryGetValue<string>(out var parsed) && !string.IsNullOrWhiteSpace(parsed))
            {
                set(settings, parsed);

                return true;
            }

            return false;
        };
        _readers[clearName] = (node, settings) =>
        {
            if (node is JsonValue value && value.TryGetValue<bool>(out var parsed))
            {
                if (parsed)
                {
                    clear(settings);
                }

                return true;
            }

            return false;
        };
        _validators.Add((values, _, errors) =>
        {
            if (values.ContainsKey(name) && values[clearName] is JsonValue value && value.TryGetValue<bool>(out var parsed) && parsed)
            {
                errors[name] = [S["Do not set and clear {0} in the same request.", name]];
            }
        });
    }

    /// <summary>
    /// Adds a secret stored with data protection, optionally as a reference to a secret of the Secrets module. A new
    /// value replaces the reference, and <c>{name}SecretName</c> reads the name of the referenced secret back.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="description">The schema description.</param>
    /// <param name="protector">The protector the feature reads the stored value with.</param>
    /// <param name="getProtected">Reads the protected value.</param>
    /// <param name="setProtected">Writes the protected value.</param>
    /// <param name="getSecretName">Reads the name of the referenced secret, for settings that can reference one.</param>
    /// <param name="setSecretName">Writes the name of the referenced secret, for settings that can reference one.</param>
    public void ProtectedSecret(
        string name,
        string description,
        IDataProtector protector,
        Func<TSettings, string> getProtected,
        Action<TSettings, string> setProtected,
        Func<TSettings, string> getSecretName = null,
        Action<TSettings, string> setSecretName = null)
    {
        Secret(
            name,
            description,
            settings => !string.IsNullOrEmpty(getSecretName?.Invoke(settings)) || HasProtectedValue(protector, getProtected(settings)),
            (settings, value) =>
            {
                // Protecting a value gives a new payload every time, which an equivalent retry shouldn't store.
                if (!Matches(protector, getProtected(settings), value))
                {
                    setProtected(settings, protector.Protect(value));
                }

                setSecretName?.Invoke(settings, null);
            },
            settings =>
            {
                setProtected(settings, null);
                setSecretName?.Invoke(settings, null);
            });

        if (getSecretName is not null)
        {
            _writers.Add((settings, values) => values[name + "SecretName"] = getSecretName(settings));
        }
    }

    internal JsonObject GetSchema() => _schema.DeepClone().AsObject();

    internal JsonObject GetValues(TSettings settings)
    {
        var values = new JsonObject();
        foreach (var writer in _writers)
        {
            writer(settings, values);
        }

        return values;
    }

    internal void Apply(JsonObject values, TSettings proposed, Dictionary<string, string[]> errors)
    {
        foreach (var entry in values)
        {
            if (!_readers.TryGetValue(entry.Key, out var reader))
            {
                errors[entry.Key] = [S["This property is not managed by this settings section."]];
            }
            else if (!reader(entry.Value, proposed))
            {
                errors[entry.Key] = [S["Provide a valid value of the documented type."]];
            }
        }

        foreach (var validator in _validators)
        {
            validator(values, proposed, errors);
        }
    }

    private static bool TryParseEnum<TEnum>(JsonNode node, out TEnum parsed)
        where TEnum : struct, Enum
    {
        parsed = default;

        return node is JsonValue value && value.TryGetValue<string>(out var name)
            && System.Enum.GetNames<TEnum>().Contains(name, StringComparer.OrdinalIgnoreCase)
            && System.Enum.TryParse(name, ignoreCase: true, out parsed);
    }

    private static bool HasProtectedValue(IDataProtector protector, string protectedValue)
    {
        if (string.IsNullOrEmpty(protectedValue))
        {
            return false;
        }

        try
        {
            return !string.IsNullOrEmpty(protector.Unprotect(protectedValue));
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // A value that can no longer be decrypted is still stored.
            return true;
        }
    }

    private static bool Matches(IDataProtector protector, string protectedValue, string value)
    {
        if (string.IsNullOrEmpty(protectedValue))
        {
            return false;
        }

        try
        {
            return string.Equals(protector.Unprotect(protectedValue), value, StringComparison.Ordinal);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // Replace credentials that can no longer be decrypted.
            return false;
        }
    }
}
