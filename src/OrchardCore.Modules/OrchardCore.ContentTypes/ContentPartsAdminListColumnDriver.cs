using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.ContentTypes.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes;

/// <summary>
/// Declares the columns of the <see cref="ContentPartsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. There is no selection column: the page has no bulk actions to apply to the selected rows.
/// </summary>
public sealed class ContentPartsAdminListColumnDriver : AdminListColumnDriver<ContentPartEntry>
{
    private readonly IStringLocalizer S;

    public ContentPartsAdminListColumnDriver(IStringLocalizer<ContentPartsAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
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

        return null;
    }
}
