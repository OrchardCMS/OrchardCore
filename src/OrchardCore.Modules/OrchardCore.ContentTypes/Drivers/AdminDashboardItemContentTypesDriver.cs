using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers
{
    public class AdminDashboardItemContentTypesDriver : DisplayDriver<AdminDashboardItem>
    {
        public override IDisplayResult Display(AdminDashboardItem model)
        {
            return Dynamic("AdminDashboardItemContentTypes_SummaryAdmin").Location("SummaryAdmin", "Content:12");
        }
    }
}
