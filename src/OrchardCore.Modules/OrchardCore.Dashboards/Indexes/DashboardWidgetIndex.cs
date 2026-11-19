using OrchardCore.ContentManagement;
using OrchardCore.Dashboards.Models;
using YesSql.Indexes;

namespace OrchardCore.Dashboards.Indexes;

/// <summary>
/// Indexes the widgets of the dashboards, by dashboard type, owner and position.
/// </summary>
public class DashboardWidgetIndex : MapIndex
{
    public const int MaxDashboardTypeSize = 255;

    public string DashboardType { get; set; }

    public string OwnerId { get; set; }

    public double Position { get; set; }
}

public sealed class DashboardWidgetIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<DashboardWidgetIndex>()
            .Map(contentItem =>
            {
                // Store only published and latest versions.
                if (!contentItem.Published && !contentItem.Latest)
                {
                    return null;
                }

                if (contentItem.TryGet<DashboardPart>(out var dashboardPart))
                {
                    return new DashboardWidgetIndex
                    {
                        DashboardType = dashboardPart.DashboardType,
                        OwnerId = dashboardPart.OwnerId,
                        Position = dashboardPart.Position,
                    };
                }

                return null;
            });
    }
}
