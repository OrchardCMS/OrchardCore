using System.Text;
using Json.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public class ServiceProviderRecipeHarvesterTests
{
    [Fact]
    public async Task HarvestRecipesAsync_ShouldReturnRecipes_FromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRecipeDescriptor, TestRecipeDescriptor>();

        var provider = services.BuildServiceProvider();

        var harvester = new ServiceProviderRecipeHarvester(provider.GetServices<IRecipeDescriptor>());
        var recipes = (await harvester.HarvestRecipesAsync()).ToList();

        Assert.Single(recipes);
        Assert.Equal("TestCodeRecipe", recipes[0].Name);
    }

    [Fact]
    public async Task HarvestRecipesAsync_ShouldSupportSchemas_FromServiceProviderDescriptors()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRecipeDescriptor, TestRecipeDescriptor>();

        var provider = services.BuildServiceProvider();

        var serviceProviderDescriptor = provider.GetRequiredService<IRecipeDescriptor>();
        var schema = await serviceProviderDescriptor.GetSchemaAsync();

        Assert.NotNull(schema);
        Assert.Equal("TestCodeRecipe", schema.GetTitle());
    }

    [Fact]
    public async Task FileRecipeDescriptor_ShouldLoadSchemaFile_WhenPresent()
    {
        var recipeJson = "{\"name\":\"FileRecipe\",\"steps\":[{\"name\":\"Content\"}]}";
        var schemaJson = "{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"title\":\"FileRecipeSchema\",\"type\":\"object\"}";

        var fileProvider = new InMemoryFileProvider(new Dictionary<string, string>
        {
            ["Recipes/FileRecipe.recipe.json"] = recipeJson,
            ["Recipes/FileRecipe.recipe.schema.json"] = schemaJson,
        });

        var recipeFileInfo = fileProvider.GetFileInfo("Recipes/FileRecipe.recipe.json");

        var schemaService = new RecipeSchemaService([]);
        var descriptor = new FileRecipeDescriptor("Recipes", recipeFileInfo, fileProvider, schemaService, NullLogger<FileRecipeDescriptor>.Instance)
        {
            Name = "FileRecipe",
        };

        var schema = await descriptor.GetSchemaAsync();

        Assert.NotNull(schema);
        Assert.Equal("FileRecipeSchema", schema.GetTitle());
    }

    private sealed class TestRecipeDescriptor : CodeRecipeDescriptor
    {
        public override string Name => "TestCodeRecipe";
        public override string DisplayName => "Test Code Recipe";

        protected override IEnumerable<IRecipeDeploymentStep> BuildSteps() => [];

        protected override JsonSchema BuildSchema() => new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Title(Name)
            .Type(SchemaValueType.Object)
            .Build();

        protected override System.Text.Json.Nodes.JsonObject BuildRecipeJson() => new()
        {
            ["name"] = Name,
            ["steps"] = new System.Text.Json.Nodes.JsonArray(),
        };
    }

    private sealed class InMemoryFileProvider : IFileProvider
    {
        private readonly Dictionary<string, string> _files;

        public InMemoryFileProvider(Dictionary<string, string> files)
        {
            _files = files;
        }

        public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath)
        {
            if (_files.TryGetValue(Normalize(subpath), out var content))
            {
                return new InMemoryFileInfo(Path.GetFileName(subpath), content);
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        private static string Normalize(string path) => path.Replace('\\', '/');

        private sealed class InMemoryFileInfo : IFileInfo
        {
            private readonly byte[] _bytes;

            public InMemoryFileInfo(string name, string content)
            {
                Name = name;
                _bytes = Encoding.UTF8.GetBytes(content);
            }

            public bool Exists => true;
            public long Length => _bytes.Length;
            public string PhysicalPath => null;
            public string Name { get; }
            public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
            public bool IsDirectory => false;
            public Stream CreateReadStream() => new MemoryStream(_bytes, writable: false);
        }
    }
}
