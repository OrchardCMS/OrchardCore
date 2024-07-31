using System.Threading.Tasks;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Admin.Drivers;

public class VisitSiteNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override Task<IDisplayResult> DisplayAsync(Navbar model, BuildDisplayContext context)
    {
        return Task.FromResult<IDisplayResult>(
            View("VisitSiteNavbarItem", model)
            .Location("DetailAdmin", "Content:20")
        );
    }
}
