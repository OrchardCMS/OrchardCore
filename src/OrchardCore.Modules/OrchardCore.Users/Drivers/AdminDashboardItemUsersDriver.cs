using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Drivers
{
    public class AdminDashboardItemUsersDriver : DisplayDriver<AdminDashboardItem>
    {
        public override IDisplayResult Display(AdminDashboardItem model)
        {
            return Dynamic("AdminDashboardItemUsers_SummaryAdmin").Location("SummaryAdmin", "Content:20");
        }
    }
}
