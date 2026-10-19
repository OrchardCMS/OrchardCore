using System.Security.Claims;
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.ViewModels;

/// <summary>
/// A data set offered by the editors and the designer, with the localized name of its data source.
/// </summary>
public sealed class DataSetOption
{
    /// <summary>
    /// The separator of the data source and the data set in <see cref="Value"/>.
    /// </summary>
    public const string Separator = "::";

    /// <summary>
    /// Gets or sets the data source and the data set, such as <c>Contents::BlogPost</c>.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the label of the data set.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the description of the data set, if any.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the localized name of the data source.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the group the data set is listed under in its data source, if any.
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// Joins a data source and a data set into a <see cref="Value"/>.
    /// </summary>
    /// <param name="source">The technical name of the data source.</param>
    /// <param name="dataSet">The technical name of the data set.</param>
    /// <returns>The value, or <see langword="null"/> when the data source is empty.</returns>
    public static string ToValue(string source, string dataSet)
        => string.IsNullOrEmpty(source) ? null : source + Separator + dataSet;

    /// <summary>
    /// Splits a <see cref="Value"/> into its data source and data set.
    /// </summary>
    /// <param name="value">The value, such as <c>Contents::BlogPost</c>.</param>
    /// <returns>The data source and the data set, both <see langword="null"/> when the value has no separator.</returns>
    public static (string Source, string DataSet) Parse(string value)
    {
        var parts = (value ?? string.Empty).Split(Separator, 2);

        return parts.Length == 2 ? (parts[0], parts[1]) : (null, null);
    }

    /// <summary>
    /// Lists the data sets of every data source that a user may read, in the order of the data sources.
    /// </summary>
    /// <param name="dataSourceManager">The data sources.</param>
    /// <param name="localizerFactory">Localizes the names of the data sources.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data sets.</returns>
    public static async Task<IReadOnlyList<DataSetOption>> ListAsync(
        IDataSourceManager dataSourceManager,
        IStringLocalizerFactory localizerFactory,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataSourceManager);
        ArgumentNullException.ThrowIfNull(localizerFactory);

        var options = new List<DataSetOption>();
        var context = new DataSourceContext { User = user };

        foreach (var descriptor in dataSourceManager.GetDataSources())
        {
            var sourceName = localizerFactory.Localize(descriptor.DisplayName)?.Value ?? descriptor.Name;

            foreach (var dataSet in await dataSourceManager.GetDataSource(descriptor.Name).GetDataSetsAsync(context, cancellationToken))
            {
                options.Add(new DataSetOption
                {
                    Value = ToValue(descriptor.Name, dataSet.Name),
                    Text = dataSet.DisplayName,
                    Description = dataSet.Description,
                    Source = sourceName,
                    Group = dataSet.Group,
                });
            }
        }

        return options;
    }
}
