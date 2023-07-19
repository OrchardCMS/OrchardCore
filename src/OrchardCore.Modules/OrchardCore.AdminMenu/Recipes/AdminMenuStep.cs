using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.AdminMenu.Recipes
{
    /// <summary>
    /// This recipe step creates a set of admin menus.
    /// </summary>
    public class AdminMenuStep : IRecipeStepHandler
    {
        private readonly IAdminMenuService _adminMenuService;

        public AdminMenuStep(IAdminMenuService adminMenuService)
        {
            _adminMenuService = adminMenuService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "AdminMenu", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<AdminMenuStepModel>();

            var serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var token in model.Data.Cast<JObject>())
            {
                var adminMenu = token.ToObject<Models.AdminMenu>(serializer);

                // When the id is not supplied generate an id, otherwise replace the menu if it exists, or create a new menu.
                if (String.IsNullOrEmpty(adminMenu.Id))
                {
                    adminMenu.Id = Guid.NewGuid().ToString("n");
                }

                await _adminMenuService.SaveAsync(adminMenu);
            }

            return;
        }
    }

    public class AdminMenuStepModel
    {
        public JArray Data { get; set; }
    }
}
