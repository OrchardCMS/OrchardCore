using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Deployment;

/// <summary>
/// Declares the columns of the <see cref="DeploymentPlansAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>DeploymentPlanEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class DeploymentPlansAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public DeploymentPlansAdminListColumnProvider(IStringLocalizer<DeploymentPlansAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName != DeploymentPlansAdminList.Name)
        {
            return Task.CompletedTask;
        }

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

        return Task.CompletedTask;
    }
}
