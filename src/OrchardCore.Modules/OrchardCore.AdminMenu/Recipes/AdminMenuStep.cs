using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Json;
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
        private readonly JsonSerializerOptions _serializationOptions;

        public AdminMenuStep(
            IAdminMenuService adminMenuService,
            IOptions<ContentSerializerJsonOptions> serializationOptions)
        {
            _adminMenuService = adminMenuService;

            // The recipe step contains polymorphic types (menu items) which need to be resolved
            _serializationOptions = serializationOptions.Value.SerializerOptions;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "AdminMenu", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<AdminMenuStepModel>(_serializationOptions);

            foreach (var token in model.Data.Cast<JsonObject>())
            {
                var adminMenu = token.ToObject<Models.AdminMenu>(_serializationOptions);

                // When the id is not supplied generate an id, otherwise replace the menu if it exists, or create a new menu.
                if (string.IsNullOrEmpty(adminMenu.Id))
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
        public JsonArray Data { get; set; }
    }
}
