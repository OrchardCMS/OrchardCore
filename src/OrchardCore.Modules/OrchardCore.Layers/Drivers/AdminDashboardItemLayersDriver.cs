using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Layers.Drivers
{
    public class AdminDashboardItemLayersDriver : DisplayDriver<AdminDashboardItem>
    {
        public override IDisplayResult Display(AdminDashboardItem model)
        {
            return Dynamic("AdminDashboardItemLayers_SummaryAdmin").Location("SummaryAdmin", "Content:16");
        }
    }
}
