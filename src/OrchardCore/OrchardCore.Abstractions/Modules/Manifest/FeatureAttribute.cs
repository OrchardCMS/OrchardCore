namespace OrchardCore.Modules.Manifest;

/// <summary>
/// Defines a Feature in a Module, can be used multiple times.
/// If at least one Feature is defined, the Module default feature is ignored.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class FeatureAttribute : Attribute
{
    private string _id;
    private string _name;
    private string _category = "";
    private string[] _dependencies = [];

    // Gets the default known ListDelimiters supporting Dependencies splits, etc.
    // Semi-colon ';' delimiters are most common, expected from a CSPROJ
    // perspective. Also common are comma ',' and space ' '
    // delimiters.
    protected internal static readonly char[] ListDelimiters = [';', ',', ' '];

    /// <summary>
    /// Default parameterless ctor.
    /// </summary>
    public FeatureAttribute()
    {
    }

    /// <summary>
    /// Constructs an instance of the attribute with some default values.
    /// </summary>
    /// <param name="id">An identifier overriding the Name.</param>
    /// <param name="description">A simple feature description.</param>
    /// <param name="featureDependencies">Zero or more delimited feature dependencies,
    /// corresponding to each of the feature <see cref="Name"/> properties.</param>
    /// <param name="defaultTenant">Whether considered default tenant only.</param>
    /// <param name="alwaysEnabled">Whether feature is always enabled.</param>
    /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
    /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
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
    /// Constructs an instance of the attribute with some default values.
    /// </summary>
    /// <param name="id">An identifier overriding the Name.</param>
    /// <param name="name">The feature name, defaults to <see cref="Id"/> when null or
    /// blank.</param>
    /// <param name="description">A simple feature description.</param>
    /// <param name="featureDependencies">Zero or more delimited feature dependencies,
    /// corresponding to each of the feature <see cref="Name"/> properties.</param>
    /// <param name="defaultTenant">Whether considered default tenant only.
    /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
    /// <param name="alwaysEnabled">Whether feature is always enabled.
    /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
    /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
    /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
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
    /// Constructs an instance of the attribute with some default values.
    /// </summary>
    /// <param name="id">An identifier overriding the Name.</param>
    /// <param name="name">The feature name, defaults to <see cref="Id"/> when null or
    /// blank.</param>
    /// <param name="category">A simple feature category.</param>
    /// <param name="priority">The priority of the Feature.</param>
    /// <param name="description">A simple feature description.</param>
    /// <param name="featureDependencies">Zero or more delimited feature dependencies,
    /// corresponding to each of the feature <see cref="Name"/> properties.</param>
    /// <param name="defaultTenant">Whether considered default tenant only.
    /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
    /// <param name="alwaysEnabled">Whether feature is always enabled.
    /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
    /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
    /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
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
        Dependencies = ParseDependencies(featureDependencies);
        DefaultTenantOnly = Convert.ToBoolean(defaultTenant);
        IsAlwaysEnabled = Convert.ToBoolean(alwaysEnabled);
        EnabledByDependencyOnly = Convert.ToBoolean(enabledByDependencyOnly);
    }

    /// <summary>
    /// Whether the feature exists based on the <see cref="Id"/>.
    /// </summary>
    public virtual bool Exists => !string.IsNullOrEmpty(Id);

    /// <summary>
    /// Gets or sets the feature identifier. Identifier is required.
    /// </summary>
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
    /// Gets or sets the human readable or canonical feature name. <see cref="Id"/> will be
    /// returned when not provided or blank.
    /// </summary>
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
    /// Gets or sets the Category for use with the Module.
    /// </summary>
    public virtual string Category
    {
        get => _category;
        set => _category = value?.Trim() ?? "";
    }

    /// <summary>
    /// Gets or sets the feature priority without breaking the <see cref="Dependencies"/>
    /// order. The higher is the priority, the later the drivers / handlers are invoked.
    /// </summary>
    /// <remarks>
    /// The default value is 0, consistent with the baseline, however, could
    /// be nullified, which would in turn favor the parent <see cref="ModuleAttribute"/>.
    /// </remarks>
    public virtual string Priority { get; set; } = "0";

    /// <summary>
    /// Gets the <see cref="Priority"/>, parsed and ready to go for Internal use. May yield
    /// <c>null</c> when failing to <see cref="int.TryParse(string, out int)"/>.
    /// </summary>
    internal int? InternalPriority => int.TryParse(Priority, out var result) ? result : null;

    /// <summary>
    /// Gets or sets an array of Feature Dependencies. Used to arrange drivers, handlers
    /// invoked during startup and so forth.
    /// </summary>
    public virtual string[] Dependencies
    {
        get => _dependencies;
        set => _dependencies = value?.Select(d => d.Trim()).ToArray() ?? [];
    }

    /// <summary>
    /// Set to <c>true</c> to only allow the <em>Default tenant to enable or disable</em> the feature.
    /// </summary>
    public virtual bool DefaultTenantOnly { get; set; }

    /// <summary>
    /// Once enabled, check whether the feature cannot be disabled. Defaults to <c>false</c>.
    /// </summary>
    public virtual bool IsAlwaysEnabled { get; set; }

    /// <summary>
    /// Set to <c>true</c> to make the feature available by dependency only.
    /// </summary>
    public virtual bool EnabledByDependencyOnly { get; set; }

    /// <summary>
    /// Describes the first or default Feature starting with This instance,
    /// which defines a <see cref="Description"/>.
    /// </summary>
    /// <param name="additionalFeatures">Additional Features to consider in the aggregate.</param>
    /// <returns>The first or default Description with optional back stop features.</returns>
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
    /// Categorizes This <see cref="Category"/> using <paramref name="additionalFeatures"/> as
    /// back stops, presents the <see cref="Category"/> that is not Null nor Empty, or returns
    /// "Uncategorized" by default.
    /// </summary>
    /// <param name="additionalFeatures">Additional Feature instances to use as potential back stops.</param>
    /// <returns>The Category normalized across This instance and optional Module.</returns>
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
    /// Prioritizes the Features starting with This one, concatenating
    /// <paramref name="additionalFeatures"/>, and lifting the <see cref="InternalPriority"/>
    /// from there. We prefer the first non Null Priority, default
    /// 0.
    /// </summary>
    /// <param name="additionalFeatures"></param>
    /// <returns></returns>
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

    private static string[] ParseDependencies(string dependencies)
    {
        if (string.IsNullOrWhiteSpace(dependencies))
        {
            return [];
        }

        return dependencies.Split(ListDelimiters, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
