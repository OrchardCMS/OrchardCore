using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Menu.Models;
using YesSql.Core.Services;

namespace Orchard.Menu.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly ISession _session;

        public AdminController(
            ISession session,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _session = session;
        }

        public async Task<IActionResult> Create(string id, int menuContentItemId, int menuItemId)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
            {
                return Unauthorized();
            }

            var contentItem = _contentManager.New(id);

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this);

            model.MenuContentItemId = menuContentItemId;
            model.MenuItemId = menuItemId;

            return View(model);
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(string id, int menuContentItemId, int menuItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
            {
                return Unauthorized();
            }

            var menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);

            if (menu == null)
            {
                return NotFound();
            }

            var contentItem = _contentManager.New(id);

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this);

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            if (menuItemId == 0)
            {
                // Use the menu as the parent if no target is specified
                menu.Alter<MenuItemsListPart>(part => part.MenuItems.Add(contentItem));
            }
            else
            {
                // Look for the target menu item in the hierarchy
                var parentMenuItem = FindMenuItem(menu.Content, menuItemId);

                // Couldn't find targetted menu item
                if (parentMenuItem == null)
                {
                    return NotFound();
                }

                var menuItems = parentMenuItem?.MenuItemsListPart?.MenuItems as JArray;

                if (menuItems == null)
                {
                    parentMenuItem["MenuItemsListPart"] = new JObject(
                        new JProperty("MenuItems", menuItems = new JArray())
                        );
                }

                menuItems.Add(JObject.FromObject(contentItem));
            }

            _session.Save(menu);

            return RedirectToAction("Edit", "Admin", new { area = "Orchard.Contents", id = menuContentItemId });
        }

        private JObject FindMenuItem(JObject contentItem, int menuItemId)
        {
            if (contentItem["ContentItemId"]?.Value<int>() == menuItemId)
            {
                return contentItem;
            }

            if (contentItem.GetValue("MenuItemsListPart") == null)
            {
                return null;
            }

            var menuItems = (JArray) contentItem["MenuItemsListPart"]["MenuItems"];

            JObject result;

            foreach(JObject menuItem in menuItems)
            {
                // Search in inner menu items
                result = FindMenuItem(menuItem, menuItemId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
