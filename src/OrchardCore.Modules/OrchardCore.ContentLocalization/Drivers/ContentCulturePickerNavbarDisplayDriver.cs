using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentLocalization.Drivers;

public class ContentCulturePickerNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model)
    {
        return View("ContentCulturePicker", model)
            .Location("Detail", "Content:5");
    }
}
