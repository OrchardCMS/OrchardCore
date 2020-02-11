using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.Handlers
{
    public class MenuContentHandler : ContentHandlerBase
    {
        public override Task ActivatedAsync(ActivatedContentContext context)
        {
            // When a Menu is created, we add a MenuPart to it
            if (context.ContentItem.ContentType == "Menu")
            {
                context.ContentItem.Weld<MenuPart>(new { Position = "3" });
                context.ContentItem.Weld<MenuItemsListPart>(new { Position = "4" });
            }

            return Task.CompletedTask;
        }
    }
}
