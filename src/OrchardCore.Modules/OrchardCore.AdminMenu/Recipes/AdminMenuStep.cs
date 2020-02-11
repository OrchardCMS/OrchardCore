using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.AdminMenu.Recipes
{
    /// <summary>
    /// This recipe step creates a set of admin menus.
    /// </summary>
    public class AdminMenuStep : IRecipeStepHandler
    {
        private readonly IAdminMenuService _AdminMenuService;

        public AdminMenuStep(IAdminMenuService AdminMenuervice)
        {
            _AdminMenuService = AdminMenuervice;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "AdminMenu", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<AdminMenuStepModel>();

            var serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.Auto };

            foreach (JObject token in model.Data)
            {
                var adminMenu = token.ToObject<Models.AdminMenu>(serializer);
                adminMenu.Id = Guid.NewGuid().ToString("n");// we always add it as a new tree.
                await _AdminMenuService.SaveAsync(adminMenu);
            }

            return;
        }
    }

    public class AdminMenuStepModel
    {
        public JArray Data { get; set; }
    }
}
