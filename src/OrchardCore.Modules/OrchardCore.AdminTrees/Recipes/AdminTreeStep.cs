using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.AdminTrees.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using YesSql;

namespace OrchardCore.AdminTrees.Recipes
{
    /// <summary>
    /// This recipe step creates a set of admin trees.
    /// </summary>
    public class AdminTreeStep : IRecipeStepHandler
    {
        private readonly ISession _session;

        public AdminTreeStep(ISession session)
        {
            _session = session;
        }

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "AdminTrees", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var model = context.Step.ToObject<AdminTreeStepModel>();

            var serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.Auto };

            foreach (JObject token in model.Data)
            {
                var adminTree = token.ToObject<AdminTree>(serializer);
                adminTree.Id = 0; // we always add it as a new tree.
                _session.Save(adminTree);
            }

            return Task.CompletedTask;
        }
    }

    public class AdminTreeStepModel
    {
        public JArray Data { get; set; }
    }
}