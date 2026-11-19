using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Dashboards.Models;

namespace OrchardCore.Dashboards.Handlers;

/// <summary>
/// Assigns the <see cref="DashboardOptions.DefaultType"/> to the widgets saved without a dashboard type, for example by a recipe.
/// </summary>
internal sealed class DashboardPartHandler : ContentPartHandler<DashboardPart>
{
    private readonly DashboardOptions _options;

    public DashboardPartHandler(IOptions<DashboardOptions> options)
    {
        _options = options.Value;
    }

    public override Task CreatingAsync(CreateContentContext context, DashboardPart part)
        => AssignDefaultTypeAsync(part);

    public override Task ImportingAsync(ImportContentContext context, DashboardPart part)
        => AssignDefaultTypeAsync(part);

    public override Task UpdatingAsync(UpdateContentContext context, DashboardPart part)
        => AssignDefaultTypeAsync(part);

    private Task AssignDefaultTypeAsync(DashboardPart part)
    {
        if (string.IsNullOrEmpty(part.DashboardType) && !string.IsNullOrEmpty(_options.DefaultType))
        {
            part.DashboardType = _options.DefaultType;
            part.Apply();
        }

        return Task.CompletedTask;
    }
}
