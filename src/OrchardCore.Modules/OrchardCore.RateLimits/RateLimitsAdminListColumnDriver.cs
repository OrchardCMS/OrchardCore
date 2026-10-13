using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits;

/// <summary>
/// Declares the columns of the <see cref="RateLimitsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>RateLimitPolicy_SummaryAdmin</c> shape.
/// </summary>
public sealed class RateLimitsAdminListColumnDriver : AdminListColumnDriver<RateLimitPolicy>
{
    private readonly IStringLocalizer S;

    public RateLimitsAdminListColumnDriver(IStringLocalizer<RateLimitsAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and what the policy targets take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Status",
            Position = "30",
            Title = S["Status"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Enabled",
            Position = "40",
            Title = S["Enabled"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
