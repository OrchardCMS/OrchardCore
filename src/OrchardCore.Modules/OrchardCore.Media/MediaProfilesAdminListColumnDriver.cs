using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media;

/// <summary>
/// Declares the columns of the <see cref="MediaProfilesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>MediaProfileEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class MediaProfilesAdminListColumnDriver : AdminListColumnDriver<MediaProfileEntry>
{
    private readonly IStringLocalizer S;

    public MediaProfilesAdminListColumnDriver(IStringLocalizer<MediaProfilesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and the hint take the space left by the actions.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
