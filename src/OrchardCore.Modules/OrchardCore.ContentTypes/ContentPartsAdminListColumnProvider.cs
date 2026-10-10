using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.ContentTypes;

/// <summary>
/// Declares the columns of the <see cref="ContentPartsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. There is no selection column: the page has no bulk actions to apply to the selected rows.
/// </summary>
public sealed class ContentPartsAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public ContentPartsAdminListColumnProvider(IStringLocalizer<ContentPartsAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(new AdminListColumn
        {
            // The display name and the description take the space left by the actions.
            Name = "DisplayName",
            Position = "10",
            Title = S["Display name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
