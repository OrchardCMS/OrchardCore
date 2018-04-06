using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.Handlers
{
    public class MenuContentHandler : ContentHandlerBase
    {
        public override Task ActivatingAsync(ActivatingContentContext context)
        {
            // When a Menu is created, we add a MenuPart to it
            if (context.ContentType == "Menu")
            {
                context.Builder.Weld(nameof(MenuPart), new MenuPart());
                context.Builder.Weld(nameof(MenuItemsListPart), new MenuItemsListPart());
            }

            return Task.CompletedTask;
        }
    }
}