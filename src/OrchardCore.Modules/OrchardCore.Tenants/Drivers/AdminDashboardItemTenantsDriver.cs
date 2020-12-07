using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Tenants.Drivers
{
    public class AdminDashboardItemTenantsDriver : DisplayDriver<AdminDashboardItem>
    {
        public override IDisplayResult Display(AdminDashboardItem model)
        {
            return Dynamic("AdminDashboardItemTenants_SummaryAdmin").Location("SummaryAdmin", "Content:18");
        }
    }
}
