using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.RecipeSteps;
using OrchardCore.Recipes.Services;
using Xunit;

namespace OrchardCore.Tests.Email
{
    public class RecipeStepTests
    {
        [Fact]
        public async Task RecipesShouldBeCapturedIntoContext()
        {
            var step = CreateRecipeStepExecutor();
            var context = new RecipeExecutionContext()
            {
                Name = "Recipes",
                Step = JObject.Parse(@"{""values"":[
                                                    {""name"":""subrecipe"",""executionid"":""something""},
                                                    {""name"":""subrecipe2""},
                                                ]}")
            };
            await step.ExecuteAsync(context);
            Assert.Collection(context.InnerRecipes,
                item => Assert.Equal("subrecipe", item.Name),
                item2 => Assert.Equal("subrecipe2", item2.Name)
            );
        }

        private static RecipesStep CreateRecipeStepExecutor()
        {
            var options = new Mock<IRecipeHarvester>();
            options.Setup(o => o.HarvestRecipesAsync()).ReturnsAsync(new[] { new RecipeDescriptor { Name = "subrecipe" }, new RecipeDescriptor { Name = "subrecipe2" } });
            var smtp = new RecipesStep(new[] { options.Object });
            return smtp;
        }
    }
}
