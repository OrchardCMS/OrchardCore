using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.AdminMenu.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using YesSql;

namespace OrchardCore.AdminMenu.Recipes
{
    /// <summary>
    /// This recipe step creates a set of admin trees.
    /// </summary>
    public class AdminMenutep : IRecipeStepHandler
    {
        private readonly IAdminMenuervice _AdminMenuervice;

        public AdminMenutep(IAdminMenuervice AdminMenuervice)
        {
            _AdminMenuervice = AdminMenuervice;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "AdminMenu", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<AdminMenutepModel>();

            var serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.Auto };

            foreach (JObject token in model.Data)
            {
                var adminTree = token.ToObject<AdminTree>(serializer);
                adminTree.Id = Guid.NewGuid().ToString("n");// we always add it as a new tree.
                await _AdminMenuervice.SaveAsync(adminTree);
            }

            return;
        }
    }

    public class AdminMenutepModel
    {
        public JArray Data { get; set; }
    }
}