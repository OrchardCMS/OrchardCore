using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin;

/// <summary>
/// A row of an admin list: the item it renders, which the column drivers build its cells from, and the row shape
/// the page built for it, usually with the <c>SummaryAdmin</c> display type.
/// </summary>
/// <typeparam name="TModel">The items of the list, e.g. <c>User</c>.</typeparam>
/// <param name="Model">The item of the row.</param>
/// <param name="Shape">
/// The row shape. The <c>List</c> layout renders it whole, and the layouts with columns render its zones, its
/// classes and its attributes, e.g. <c>data-filter-value</c> for a client-side search.
/// </param>
public sealed record AdminListRow<TModel>(TModel Model, IShape Shape);
