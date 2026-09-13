using System.Text;
using OrchardCore.Recipes.Endpoints.Management;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Recipes;

public class RecipeManagementEndpointsTests
{
    [Fact]
    public void GetParameters_MergesRecipeVariablesAndEnvironmentUsingStableRecipeId()
    {
        var recipe = new RecipeDescriptor
        {
            BasePath = "Recipes/Module",
            RecipeFileInfo = new TestFileInfo("sample.recipe.json", """
                {
                  "variables": {
                    "Color": "Blue",
                    "Region": "US"
                  }
                }
                """),
        };

        var recipeId = RecipeManagementEndpoints.GetRecipeId(recipe);
        var parameters = RecipeManagementEndpoints.GetParameters(recipe, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["Region"] = "EU",
            ["Tenant"] = "Default",
        });

        Assert.Equal("UmVjaXBlcy9Nb2R1bGV8c2FtcGxlLnJlY2lwZS5qc29u", recipeId);

        Assert.Collection(parameters,
            parameter =>
            {
                Assert.Equal("Color", parameter.Name);
                Assert.Equal("recipe", parameter.Source);
                Assert.Equal("Blue", parameter.DefaultValue);
                Assert.Null(parameter.CurrentValue);
            },
            parameter =>
            {
                Assert.Equal("Region", parameter.Name);
                Assert.Equal("recipe+environment", parameter.Source);
                Assert.Equal("US", parameter.DefaultValue);
                Assert.Equal("EU", parameter.CurrentValue);
            },
            parameter =>
            {
                Assert.Equal("Tenant", parameter.Name);
                Assert.Equal("environment", parameter.Source);
                Assert.Null(parameter.DefaultValue);
                Assert.Equal("Default", parameter.CurrentValue);
            });
    }

    private sealed class TestFileInfo(string name, string content) : IFileInfo
    {
        private readonly byte[] _content = Encoding.UTF8.GetBytes(content);

        public bool Exists => true;
        public long Length => _content.Length;
        public string PhysicalPath => null;
        public string Name { get; } = name;
        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
        public bool IsDirectory => false;

        public Stream CreateReadStream() => new MemoryStream(_content, writable: false);
    }
}
