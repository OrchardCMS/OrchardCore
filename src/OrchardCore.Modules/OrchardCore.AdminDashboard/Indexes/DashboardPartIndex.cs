using OrchardCore.AdminDashboard.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.AdminDashboard.Indexes
{
    public class DashboardPartIndex : MapIndex
    {
        public double Position { get; set; }
    }

    public class DashboardPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<DashboardPartIndex>()
                .Map(contentItem =>
                {
                    var dashboardPart = contentItem.As<DashboardPart>();

                    // Store only published and latest versions
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }

                    if (dashboardPart != null)
                    {
                        return new DashboardPartIndex
                        {
                            Position = dashboardPart.Position,
                        };
                    }

                    return null;
                });
        }
    }
}
