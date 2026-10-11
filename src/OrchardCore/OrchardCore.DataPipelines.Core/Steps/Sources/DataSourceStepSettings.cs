using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="DataSourceStep"/>.
/// </summary>
public sealed class DataSourceStepSettings
{
    /// <summary>
    /// Gets or sets the technical name of the data source.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the technical name of the data set.
    /// </summary>
    public string DataSet { get; set; }

    /// <summary>
    /// Gets or sets the fields to read, in order. When empty, every field is read.
    /// </summary>
    public List<string> Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets the conditions the rows must match.
    /// </summary>
    public List<DataSourceFilter> Filters { get; set; } = [];

    /// <summary>
    /// Gets or sets the most rows to read, or <c>0</c> to read every row.
    /// </summary>
    public int MaxRows { get; set; }

    /// <summary>
    /// Gets or sets the values of the parameters of a data set that takes them, such as a saved query.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = [];
}

/// <summary>
/// A condition the rows of a <see cref="DataSourceStep"/> must match.
/// </summary>
public sealed class DataSourceFilter
{
    /// <summary>
    /// Gets or sets the field to compare.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the comparison.
    /// </summary>
    public DataFilterOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the values to compare with, as invariant text: numbers with a dot, dates as <c>yyyy-MM-dd</c>.
    /// </summary>
    public List<string> Values { get; set; } = [];
}
