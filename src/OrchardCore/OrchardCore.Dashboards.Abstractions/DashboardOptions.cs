using OrchardCore.Localization;

namespace OrchardCore.Dashboards;

/// <summary>
/// The types of dashboards registered by the enabled features, see <see cref="DashboardTypeDescriptor"/>.
/// </summary>
public sealed class DashboardOptions
{
    private readonly Dictionary<string, DashboardTypeDescriptor> _types = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the registered types, by name.
    /// </summary>
    public IReadOnlyDictionary<string, DashboardTypeDescriptor> Types => _types;

    /// <summary>
    /// Gets or sets the name of the type assigned to a widget saved without one, for example by a recipe.
    /// </summary>
    public string DefaultType { get; set; }

    /// <summary>
    /// Registers a type of dashboard, or replaces the type registered with the same name.
    /// </summary>
    /// <param name="name">The technical name of the type, stored on its widgets and used in its URLs.</param>
    /// <param name="displayName">The name shown to users, as the title of the dashboard.</param>
    /// <param name="configure">An optional delegate to configure the type.</param>
    /// <returns>The registered type.</returns>
    public DashboardTypeDescriptor Add(string name, LocalizationSource displayName, Action<DashboardTypeDescriptor> configure = null)
    {
        var descriptor = new DashboardTypeDescriptor(name, displayName);

        configure?.Invoke(descriptor);

        _types[name] = descriptor;

        return descriptor;
    }
}
