namespace OrchardCore.Modules.Manifest;

/// <summary>
/// Defines a feature within a module. This attribute can be applied multiple times to an assembly.
/// </summary>
/// <remarks>
/// <para>
/// When at least one <see cref="FeatureAttribute"/> is defined on an assembly, 
/// the module's default feature is ignored and only the explicitly defined features are used.
/// </para>
/// <para>
/// Features enable modular functionality within OrchardCore, allowing selective enabling/disabling 
/// of capabilities and managing dependencies between different parts of the system.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class FeatureAttribute : Attribute
{
    private string _id;
    private string _name;
    private string _category = "";
    private string[] _dependencies = [];

    /// <summary>
    /// Gets the default list delimiters used for parsing dependency strings.
    /// Supported delimiters are semicolon (;), comma (,), and space ( ).
    /// </summary>
    /// <remarks>
    /// Semicolon delimiters are most common from a CSPROJ perspective.
    /// </remarks>
    protected internal static readonly char[] ListDelimiters = [';', ',', ' '];

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureAttribute"/> class.
    /// </summary>
    public FeatureAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureAttribute"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique feature identifier.</param>
    /// <param name="description">A brief description of what the feature does.</param>
    /// <param name="featureDependencies">A delimited string of feature dependencies (feature IDs).</param>
    /// <param name="defaultTenant">A value indicating whether the feature is only available to the default tenant. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="alwaysEnabled">A value indicating whether the feature is always enabled and cannot be disabled. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="enabledByDependencyOnly">A value indicating whether the feature can only be enabled as a dependency. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    public FeatureAttribute(
        string id,
        string description,
        string featureDependencies,
        object defaultTenant,
        object alwaysEnabled,
        object enabledByDependencyOnly
    ) : this(
        id,
        default,
        default,
        default,
        description,
        featureDependencies,
        defaultTenant,
        alwaysEnabled,
        enabledByDependencyOnly
    )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureAttribute"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique feature identifier.</param>
    /// <param name="name">The human-readable feature name. Defaults to <paramref name="id"/> when null or blank.</param>
    /// <param name="description">A brief description of what the feature does.</param>
    /// <param name="featureDependencies">A delimited string of feature dependencies (feature IDs).</param>
    /// <param name="defaultTenant">A value indicating whether the feature is only available to the default tenant. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="alwaysEnabled">A value indicating whether the feature is always enabled and cannot be disabled. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="enabledByDependencyOnly">A value indicating whether the feature can only be enabled as a dependency. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    public FeatureAttribute(
        string id,
        string name,
        string description,
        string featureDependencies,
        object defaultTenant,
        object alwaysEnabled,
        object enabledByDependencyOnly
    ) : this(
        id,
        name,
        default,
        default,
        description,
        featureDependencies,
        defaultTenant,
        alwaysEnabled,
        enabledByDependencyOnly
    )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureAttribute"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique feature identifier.</param>
    /// <param name="name">The human-readable feature name. Defaults to <paramref name="id"/> when null or blank.</param>
    /// <param name="category">The feature category used for grouping in the UI.</param>
    /// <param name="priority">The feature priority as a string. Higher priority features have their drivers/handlers invoked later.</param>
    /// <param name="description">A brief description of what the feature does.</param>
    /// <param name="featureDependencies">A delimited string of feature dependencies (feature IDs).</param>
    /// <param name="defaultTenant">A value indicating whether the feature is only available to the default tenant. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="alwaysEnabled">A value indicating whether the feature is always enabled and cannot be disabled. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="enabledByDependencyOnly">A value indicating whether the feature can only be enabled as a dependency. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    public FeatureAttribute(
        string id,
        string name,
        string category,
        string priority,
        string description,
        string featureDependencies,
        object defaultTenant,
        object alwaysEnabled,
        object enabledByDependencyOnly
    )
    {
        Id = id;
        Name = name;
        Category = category ?? "";
        Priority = priority ?? "";
        Description = description ?? "";
        _dependencies = ParseDependencies(featureDependencies);
        DefaultTenantOnly = Convert.ToBoolean(defaultTenant);
        IsAlwaysEnabled = Convert.ToBoolean(alwaysEnabled);
        EnabledByDependencyOnly = Convert.ToBoolean(enabledByDependencyOnly);
    }

    /// <summary>
    /// Gets a value indicating whether the feature exists based on whether the <see cref="Id"/> is set.
    /// </summary>
    public virtual bool Exists => !string.IsNullOrEmpty(Id);

    /// <summary>
    /// Gets or sets the unique feature identifier.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when attempting to set a null or empty value.</exception>
    public virtual string Id
    {
        get => _id;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);

            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the human-readable feature name.
    /// </summary>
    /// <remarks>
    /// If not set or empty, returns the <see cref="Id"/> value.
    /// </remarks>
    public virtual string Name
    {
        get => string.IsNullOrEmpty(_name) ? Id : _name;
        set => _name = value;
    }

    /// <summary>
    /// Gets or sets a brief summary of what the feature does.
    /// </summary>
    public virtual string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the category used for grouping features in the UI.
    /// </summary>
    /// <remarks>
    /// Values are trimmed when set. If not specified, features will be categorized as "Uncategorized".
    /// </remarks>
    public virtual string Category
    {
        get => _category;
        set => _category = value?.Trim() ?? "";
    }

    /// <summary>
    /// Gets or sets the feature priority as a string representation of an integer.
    /// </summary>
    /// <remarks>
    /// The priority determines the order in which drivers and handlers are invoked, 
    /// without affecting the <see cref="Dependencies"/> order. Higher priority values 
    /// result in later invocation. The default value is "0".
    /// If set to null or empty, the priority of the parent feature is used.
    /// </remarks>
    public virtual string Priority { get; set; } = "0";

    /// <summary>
    /// Gets the parsed priority value for internal use.
    /// </summary>
    /// <remarks>
    /// Returns <c>null</c> if the <see cref="Priority"/> string cannot be parsed as an integer.
    /// </remarks>
    internal int? InternalPriority => int.TryParse(Priority, out var result) ? result : null;

    /// <summary>
    /// Gets or sets the array of feature dependencies.
    /// </summary>
    /// <remarks>
    /// Dependencies are used to arrange the order in which drivers and handlers are invoked during startup.
    /// Each dependency should correspond to another feature's <see cref="Id"/>.
    /// Values are automatically trimmed when set.
    /// </remarks>
    public virtual string[] Dependencies
    {
        get => _dependencies;
        set => _dependencies = value?.Select(d => d.Trim()).ToArray() ?? [];
    }

    /// <summary>
    /// Gets or sets a value indicating whether only the default tenant can enable or disable this feature.
    /// </summary>
    public virtual bool DefaultTenantOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the feature is always enabled and cannot be disabled once activated.
    /// </summary>
    public virtual bool IsAlwaysEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the feature can only be enabled as a dependency of another feature.
    /// </summary>
    public virtual bool EnabledByDependencyOnly { get; set; }

    /// <summary>
    /// Returns the first non-empty description from this instance or the provided additional features.
    /// </summary>
    /// <param name="additionalFeatures">Additional features to check for descriptions if this instance has none.</param>
    /// <returns>The first non-empty description found, or an empty string if none exist.</returns>
    internal string Describe(params FeatureAttribute[] additionalFeatures)
    {
        if (!string.IsNullOrEmpty(Description))
        {
            return Description;
        }

        foreach (var feature in additionalFeatures)
        {
            if (!string.IsNullOrEmpty(feature?.Description))
            {
                return feature.Description;
            }
        }

        return "";
    }

    /// <summary>
    /// Returns the first non-empty category from this instance or the provided additional features.
    /// </summary>
    /// <param name="additionalFeatures">Additional features to check for categories if this instance has none.</param>
    /// <returns>The first non-empty category found, or "Uncategorized" if none exist.</returns>
    internal string Categorize(params FeatureAttribute[] additionalFeatures)
    {
        if (!string.IsNullOrEmpty(Category))
        {
            return Category;
        }

        foreach (var feature in additionalFeatures)
        {
            if (!string.IsNullOrEmpty(feature?.Category))
            {
                return feature.Category;
            }
        }

        return "Uncategorized";
    }

    /// <summary>
    /// Returns the first valid priority value from this instance or the provided additional features.
    /// </summary>
    /// <param name="additionalFeatures">Additional features to check for priority values if this instance has none.</param>
    /// <returns>The first valid priority value found, or 0 if none exist.</returns>
    internal int Prioritize(params FeatureAttribute[] additionalFeatures)
    {
        var priority = InternalPriority;
        if (priority.HasValue)
        {
            return priority.Value;
        }

        foreach (var feature in additionalFeatures)
        {
            priority = feature?.InternalPriority;
            if (priority.HasValue)
            {
                return priority.Value;
            }
        }

        return 0;
    }

    /// <summary>
    /// Parses a delimited string of dependencies into an array of trimmed dependency strings.
    /// </summary>
    /// <param name="dependencies">A string containing delimited feature dependencies.</param>
    /// <returns>An array of dependency strings, or an empty array if the input is null or whitespace.</returns>
    private static string[] ParseDependencies(string dependencies)
    {
        if (string.IsNullOrWhiteSpace(dependencies))
        {
            return [];
        }

        return dependencies.Split(ListDelimiters, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
