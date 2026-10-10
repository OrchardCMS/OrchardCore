using System.Security.Claims;

namespace OrchardCore.DataSources;

/// <summary>
/// Carries the caller information a data source needs to decide which data sets it may expose.
/// </summary>
public sealed class DataSourceContext
{
    /// <summary>
    /// Gets or sets the principal the data is read for. A data source must hide every data set this principal is not
    /// allowed to read. While a definition is designed this is the designer; while a saved definition runs unattended
    /// it is the user who vouches for it, such as its owner or the user who published it.
    /// </summary>
    public ClaimsPrincipal User { get; set; }

    /// <summary>
    /// Gets the extensible bag of values a consumer and its data sources share during one run.
    /// </summary>
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
