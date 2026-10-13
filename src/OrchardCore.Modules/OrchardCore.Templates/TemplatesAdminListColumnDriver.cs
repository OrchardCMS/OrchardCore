using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates;

/// <summary>
/// Declares the columns of the <see cref="TemplatesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>TemplateEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class TemplatesAdminListColumnDriver : AdminListColumnDriver<TemplateEntry>
{
    private readonly IStringLocalizer S;

    public TemplatesAdminListColumnDriver(IStringLocalizer<TemplatesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and the description take the space left by the actions.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
