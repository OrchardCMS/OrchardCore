namespace OrchardCore.Queries;

/// <summary>
/// The names of the breadcrumbs rendered by the queries screens, and the keys of the contextual data each of them
/// carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class QueriesConstants
{
    /// <summary>
    /// The breadcrumb of the queries list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Queries";

    /// <summary>
    /// The breadcrumb of the query creation screen. It carries <see cref="SourceNameKey"/>.
    /// </summary>
    public const string Create = "QueriesCreate";

    /// <summary>
    /// The breadcrumb of the query edition screen. It carries <see cref="QueryNameKey"/>.
    /// </summary>
    public const string Edit = "QueriesEdit";

    /// <summary>
    /// The breadcrumb of the SQL query runner.
    /// </summary>
    public const string Run = "QueriesRun";

    /// <summary>
    /// The key under which the creation screen passes the name of the source of the query.
    /// </summary>
    public const string SourceNameKey = "SourceName";

    /// <summary>
    /// The key under which the edition screen passes the name of the query.
    /// </summary>
    public const string QueryNameKey = "QueryName";
}
