using OrchardCore.ContentManagement;
using OrchardCore.AdminDashboard.Models;
using YesSql.Indexes;

namespace OrchardCore.AdminDashboard.Indexes
{
    public class DashboardPartIndex : MapIndex
    {
        //public string Zone { get; set; }
    }

    public class DashboardPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<DashboardPartIndex>()
                .Map(contentItem =>
                {
                    var dashboardMetadata = contentItem.As<DashboardMetadata>();
                    if (dashboardMetadata != null)
                    {
                        return new DashboardPartIndex
                        {
                        };
                    }

                    return null;
                });
        }
    }
}
