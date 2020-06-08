using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers
{
    public class MenuPartDisplayDriver : ContentPartDisplayDriver<MenuPart>
    {
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MenuPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IServiceProvider serviceProvider
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
        }

        public override IDisplayResult Edit(MenuPart part)
        {
            return Initialize<MenuPartEditViewModel>("MenuPart_Edit", model =>
            {
                model.MenuPart = part;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MenuPart part, IUpdateModel updater)
        {
            var model = new MenuPartEditViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, t => t.Hierarchy) && !String.IsNullOrWhiteSpace(model.Hierarchy))
            {
                var originalMenuItems = part.ContentItem.As<MenuItemsListPart>();

                var newHierarchy = JArray.Parse(model.Hierarchy);

                var menuItems = new JArray();

                foreach (var item in newHierarchy)
                {
                    menuItems.Add(ProcessItem(originalMenuItems, item as JObject));
                }

                part.ContentItem.Content["MenuItemsListPart"] = new JObject(new JProperty("MenuItems", menuItems));
            }

            return Edit(part);
        }

        /// <summary>
        /// Clone the content items at the specific index.
        /// </summary>
        private JObject GetMenuItemAt(MenuItemsListPart menuItems, int[] indexes)
        {
            ContentItem menuItem = null;

            foreach (var index in indexes)
            {
                menuItem = menuItems.MenuItems[index];
                menuItems = menuItem.As<MenuItemsListPart>();
            }

            var newObj = JObject.Parse(JsonConvert.SerializeObject(menuItem));
            if (newObj["MenuItemsListPart"] != null)
            {
                newObj["MenuItemsListPart"] = new JObject(new JProperty("MenuItems", new JArray()));
            }

            return newObj;
        }

        private JObject ProcessItem(MenuItemsListPart originalItems, JObject item)
        {
            var contentItem = GetMenuItemAt(originalItems, item["index"].ToString().Split('-').Select(x => Convert.ToInt32(x)).ToArray());

            var children = item["children"] as JArray;

            if (children != null)
            {
                var menuItems = new JArray();

                for (var i = 0; i < children.Count; i++)
                {
                    menuItems.Add(ProcessItem(originalItems, children[i] as JObject));
                    contentItem["MenuItemsListPart"] = new JObject(new JProperty("MenuItems", menuItems));
                }
            }

            return contentItem;
        }
    }
}
