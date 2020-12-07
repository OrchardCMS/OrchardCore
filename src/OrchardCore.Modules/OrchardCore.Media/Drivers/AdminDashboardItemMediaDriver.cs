using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Media.Drivers
{
    public class AdminDashboardItemMediaDriver : DisplayDriver<AdminDashboardItem>
    {
        public override IDisplayResult Display(AdminDashboardItem model)
        {
            return Dynamic("AdminDashboardItemMedia_SummaryAdmin").Location("SummaryAdmin", "Content:14");
        }
    }
}
