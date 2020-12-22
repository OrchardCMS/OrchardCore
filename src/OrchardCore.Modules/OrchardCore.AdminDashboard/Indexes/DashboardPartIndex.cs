using OrchardCore.ContentManagement;
using OrchardCore.AdminDashboard.Models;
using YesSql.Indexes;

namespace OrchardCore.AdminDashboard.Indexes
{
    public class DashboardPartIndex : MapIndex
    {
    }

    public class DashboardPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<DashboardPartIndex>()
                .Map(contentItem =>
                {
                    var dashboardPart = contentItem.As<DashboardPart>();

                    if (dashboardPart == null)
                    {
                        return null;
                    }

                    // Store only published and latest versions
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }                    
                    
                    return new DashboardPartIndex
                    {
                    };
                });
        }
    }
}
