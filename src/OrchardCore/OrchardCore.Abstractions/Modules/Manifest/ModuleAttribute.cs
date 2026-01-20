namespace OrchardCore.Modules.Manifest;

/// <summary>
/// Defines a Module which is composed of features. If the Module has only one default
/// feature, it may be defined using this attribute.
/// </summary>
/// <remarks>
/// This attribute should be applied to an assembly to define module metadata including
/// author, version, website, tags, and other properties. It extends <see cref="FeatureAttribute"/>
/// to provide module-specific functionality while maintaining feature semantics.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public class ModuleAttribute : FeatureAttribute
{
    private string _type;
    private string _author = "";
    private string _website = "";
    private string _version = "0.0";
    private string[] _tags = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleAttribute"/> class with default values.
    /// </summary>
    public ModuleAttribute() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleAttribute"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the module.</param>
    /// <param name="description">A brief description of what the module does.</param>
    /// <param name="author">The name of the module author or organization.</param>
    /// <param name="semVer">The semantic version string (e.g., "1.0.0"). See <see href="https://semver.org">Semantic Versioning</see>.</param>
    /// <param name="websiteUrl">The URL of the module's website or repository.</param>
    /// <param name="featureDependencies">A delimited string of feature dependencies (feature IDs).</param>
    /// <param name="tags">A delimited string of tags for categorizing the module.</param>
    /// <param name="defaultTenant">A value indicating whether the module is only available to the default tenant. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="alwaysEnabled">A value indicating whether the module is always enabled and cannot be disabled. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="enabledByDependencyOnly">A value indicating whether the module can only be enabled as a dependency. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    public ModuleAttribute(
        string id,
        string description,
        string author,
        string semVer,
        string websiteUrl,
        string featureDependencies,
        string tags,
        object defaultTenant,
        object alwaysEnabled,
        object enabledByDependencyOnly
    ) : this(
        id,
        default,
        default,
        default,
        description,
        author,
        semVer,
        websiteUrl,
        featureDependencies,
        tags,
        defaultTenant,
        alwaysEnabled,
        enabledByDependencyOnly
    )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleAttribute"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the module.</param>
    /// <param name="name">The human-readable module name. Defaults to <paramref name="id"/> when null or blank.</param>
    /// <param name="description">A brief description of what the module does.</param>
    /// <param name="author">The name of the module author or organization.</param>
    /// <param name="semVer">The semantic version string (e.g., "1.0.0"). See <see href="https://semver.org">Semantic Versioning</see>.</param>
    /// <param name="websiteUrl">The URL of the module's website or repository.</param>
    /// <param name="featureDependencies">A delimited string of feature dependencies (feature IDs).</param>
    /// <param name="tags">A delimited string of tags for categorizing the module.</param>
    /// <param name="defaultTenant">A value indicating whether the module is only available to the default tenant. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="alwaysEnabled">A value indicating whether the module is always enabled and cannot be disabled. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="enabledByDependencyOnly">A value indicating whether the module can only be enabled as a dependency. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    public ModuleAttribute(
        string id,
        string name,
        string description,
        string author,
        string semVer,
        string websiteUrl,
        string featureDependencies,
        string tags,
        object defaultTenant,
        object alwaysEnabled,
        object enabledByDependencyOnly
    ) : this(
        id,
        name,
        default,
        default,
        description,
        author,
        semVer,
        websiteUrl,
        featureDependencies,
        tags,
        defaultTenant,
        alwaysEnabled,
        enabledByDependencyOnly
    )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleAttribute"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the module.</param>
    /// <param name="name">The human-readable module name. Defaults to <paramref name="id"/> when null or blank.</param>
    /// <param name="category">The category used for grouping the module in the UI.</param>
    /// <param name="priority">The module priority as a string. Higher priority modules have their drivers/handlers invoked later.</param>
    /// <param name="description">A brief description of what the module does.</param>
    /// <param name="author">The name of the module author or organization.</param>
    /// <param name="semVer">The semantic version string (e.g., "1.0.0"). See <see href="https://semver.org">Semantic Versioning</see>.</param>
    /// <param name="websiteUrl">The URL of the module's website or repository.</param>
    /// <param name="featureDependencies">A delimited string of feature dependencies (feature IDs).</param>
    /// <param name="tags">A delimited string of tags for categorizing the module.</param>
    /// <param name="defaultTenant">A value indicating whether the module is only available to the default tenant. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="alwaysEnabled">A value indicating whether the module is always enabled and cannot be disabled. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="enabledByDependencyOnly">A value indicating whether the module can only be enabled as a dependency. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    public ModuleAttribute(
        string id,
        string name,
        string category,
        string priority,
        string description,
        string author,
        string semVer,
        string websiteUrl,
        string featureDependencies,
        string tags,
        object defaultTenant,
        object alwaysEnabled,
        object enabledByDependencyOnly
    ) : base(
        id,
        name,
        category,
        priority,
        description,
        featureDependencies,
        defaultTenant,
        alwaysEnabled,
        enabledByDependencyOnly
    )
    {
        Author = author;
        Website = websiteUrl;
        Version = semVer;
        _tags = ParseTags(tags);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleAttribute"/> class with the specified parameters, including a custom module type.
    /// </summary>
    /// <param name="id">The unique identifier for the module.</param>
    /// <param name="name">The human-readable module name. Defaults to <paramref name="id"/> when null or blank.</param>
    /// <param name="type">A user-defined type identifier for the module. If not provided, defaults to the attribute class name without the "Attribute" suffix.</param>
    /// <param name="category">The category used for grouping the module in the UI.</param>
    /// <param name="priority">The module priority as a string. Higher priority modules have their drivers/handlers invoked later.</param>
    /// <param name="description">A brief description of what the module does.</param>
    /// <param name="author">The name of the module author or organization.</param>
    /// <param name="semVer">The semantic version string (e.g., "1.0.0"). See <see href="https://semver.org">Semantic Versioning</see>.</param>
    /// <param name="websiteUrl">The URL of the module's website or repository.</param>
    /// <param name="featureDependencies">A delimited string of feature dependencies (feature IDs).</param>
    /// <param name="tags">A delimited string of tags for categorizing the module.</param>
    /// <param name="defaultTenant">A value indicating whether the module is only available to the default tenant. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="alwaysEnabled">A value indicating whether the module is always enabled and cannot be disabled. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    /// <param name="enabledByDependencyOnly">A value indicating whether the module can only be enabled as a dependency. Supported types are <see cref="string"/> and <see cref="bool"/>.</param>
    public ModuleAttribute(
        string id,
        string name,
        string type,
        string category,
        string priority,
        string description,
        string author,
        string semVer,
        string websiteUrl,
        string featureDependencies,
        string tags,
        object defaultTenant,
        object alwaysEnabled,
        object enabledByDependencyOnly
    ) : this(
        id,
        name,
        category,
        priority,
        description,
        author,
        semVer,
        websiteUrl,
        featureDependencies,
        tags,
        defaultTenant,
        alwaysEnabled,
        enabledByDependencyOnly
    )
    {
        type = type?.Trim();
        _type = string.IsNullOrEmpty(type) ? null : type;
    }

    /// <summary>
    /// Returns the attribute name without the "Attribute" suffix.
    /// </summary>
    /// <param name="attributeType">The attribute type to extract the prefix from.</param>
    /// <returns>The type name with the "Attribute" suffix removed if present; otherwise, the original type name.</returns>
    /// <remarks>
    /// This method is used to derive the default <see cref="Type"/> value for modules.
    /// For example, "ModuleAttribute" becomes "Module".
    /// </remarks>
    protected internal static string GetAttributePrefix(Type attributeType)
    {
        const string attributeSuffix = nameof(Attribute);

        var typeName = attributeType.Name;

        // Drops the 'Attribute' suffix from the conventional abbreviation, or leaves it alone
        return typeName.EndsWith(attributeSuffix)
            ? typeName[..^attributeSuffix.Length]
            : typeName;
    }

    /// <summary>
    /// Gets or sets the module type identifier.
    /// </summary>
    /// <value>
    /// A user-defined type that identifies the module by a logical, human-readable name.
    /// If not set, defaults to the attribute class name without the "Attribute" suffix.
    /// </value>
    /// <remarks>
    /// This allows module authors to categorize modules by custom types beyond the default classification.
    /// </remarks>
    public virtual string Type
    {
        get => _type ??= GetAttributePrefix(GetType());
        protected internal set => _type = value?.Trim();
    }

    /// <summary>
    /// Gets or sets the name of the module author or organization.
    /// </summary>
    /// <value>The author name, or an empty string if not set.</value>
    /// <remarks>Values are trimmed when set. Null values are converted to empty strings.</remarks>
    public virtual string Author
    {
        get => _author;
        set => _author = value?.Trim() ?? "";
    }

    /// <summary>
    /// Gets or sets the URL for the module's website or repository.
    /// </summary>
    /// <value>The website URL, or an empty string if not set.</value>
    /// <remarks>Values are trimmed when set. Null values are converted to empty strings.</remarks>
    public virtual string Website
    {
        get => _website;
        set => _website = value?.Trim() ?? "";
    }

    /// <summary>
    /// Gets or sets the semantic version string for the module.
    /// </summary>
    /// <value>The version string following semantic versioning conventions, or "0.0" if not set.</value>
    /// <remarks>
    /// Values are trimmed when set. Null values are converted to "0.0".
    /// See <see href="https://semver.org">Semantic Versioning</see> for version format details.
    /// </remarks>
    public virtual string Version
    {
        get => _version;
        set => _version = value?.Trim() ?? "0.0";
    }

    /// <summary>
    /// Gets or sets an array of tags associated with the module.
    /// </summary>
    /// <value>An array of tag strings, or an empty array if not set.</value>
    /// <remarks>
    /// Tags are used for categorization and discovery of modules.
    /// Values are trimmed when set. Null values are converted to empty arrays.
    /// </remarks>
    public virtual string[] Tags
    {
        get => _tags;
        set => _tags = value?.Select(t => t.Trim()).ToArray() ?? [];
    }

    /// <summary>
    /// Gets a list of feature attributes associated with this module.
    /// </summary>
    /// <value>A list of <see cref="FeatureAttribute"/> instances defining the module's features.</value>
    /// <remarks>
    /// This collection is populated when multiple features are defined for a single module.
    /// Each feature can be independently enabled or disabled.
    /// </remarks>
    public virtual List<FeatureAttribute> Features { get; } = [];

    /// <summary>
    /// Parses a delimited string of tags into an array of trimmed tag strings.
    /// </summary>
    /// <param name="tags">A string containing delimited tags.</param>
    /// <returns>An array of tag strings, or an empty array if the input is null or whitespace.</returns>
    /// <remarks>
    /// Supported delimiters are semicolon (;), comma (,), and space ( ).
    /// Empty entries are removed and all values are trimmed.
    /// </remarks>
    private static string[] ParseTags(string tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return [];
        }

        return tags.Split(ListDelimiters, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
