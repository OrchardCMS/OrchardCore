using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class ContentsAdminDashboardItemDriver : DisplayDriver<AdminDashboardItem>
    {
        public override IDisplayResult Display(AdminDashboardItem model)
        {
            return Dynamic("AdminDashboardItemContents_SummaryAdmin").Location("SummaryAdmin", "Content:10");
        }
    }
}
