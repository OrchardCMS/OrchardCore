using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Admin.Drivers;

public class VisitSiteNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model)
    {
        return View("VisitSiteNavbarItem", model)
            .Location("DetailAdmin", "Content:20");
    }
}
