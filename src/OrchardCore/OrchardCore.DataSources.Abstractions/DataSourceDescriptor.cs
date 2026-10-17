using OrchardCore.Localization;

namespace OrchardCore.DataSources;

/// <summary>
/// Describes a registered data source with the static members of its class, so data sources can be listed without
/// creating them. <c>services.AddDataSource&lt;TDataSource&gt;()</c> registers one per data source.
/// </summary>
public sealed class DataSourceDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSourceDescriptor"/> class.
    /// </summary>
    /// <param name="name">The stable technical name of the data source.</param>
    /// <param name="displayName">The label shown to users.</param>
    /// <param name="description">The description shown to users.</param>
    /// <param name="type">The class of the data source, resolved from the services when it is read.</param>
    public DataSourceDescriptor(string name, LocalizationSource displayName, LocalizationSource description, Type type)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(type);

        Name = name;
        DisplayName = displayName;
        Description = description;
        Type = type;
    }

    /// <summary>
    /// Gets the stable technical name of the data source.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the label shown to users, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the description shown to users, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource Description { get; }

    /// <summary>
    /// Gets the class of the data source, which implements <see cref="IDataSource"/>.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Describes a data source from the static members of its class.
    /// </summary>
    /// <typeparam name="TDataSource">The class of the data source.</typeparam>
    /// <returns>The descriptor.</returns>
    public static DataSourceDescriptor Create<TDataSource>()
        where TDataSource : class, IDataSource
    {
        return new DataSourceDescriptor(TDataSource.Name, TDataSource.DisplayName, TDataSource.Description, typeof(TDataSource));
    }
}
