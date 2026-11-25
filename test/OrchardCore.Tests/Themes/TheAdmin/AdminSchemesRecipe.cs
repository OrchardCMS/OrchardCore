using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Tests.Themes.TheAdmin;

/// <summary>
/// The recipe TheAdmin's migration imports its schemes from (Migrations/admin-schemes.recipe.json), read from the
/// repository.
/// </summary>
public static class AdminSchemesRecipe
{
    /// <summary>
    /// The ids of the schemes the recipe imports, in their order.
    /// </summary>
    public static readonly string[] SchemeIds = ["gray", "blue", "ocean", "forest", "plum", "sunset", "graphite"];

    /// <summary>
    /// A new copy of the 'AdminSchemes' step of the recipe.
    /// </summary>
    public static JsonObject ReadStep()
    {
        var path = Path.Combine(FindRepositoryRoot(), "src", "OrchardCore.Themes", "TheAdmin", "Migrations", "admin-schemes.recipe.json");
        var recipe = JsonNode.Parse(File.ReadAllText(path));

        return Assert.Single(recipe["steps"].AsArray()).DeepClone().AsObject();
    }

    /// <summary>
    /// The schemes of the recipe, as the step stores them.
    /// </summary>
    public static IReadOnlyList<AdminScheme> ReadSchemes()
        => [.. ReadStep()["Schemes"].AsArray().Select(entry =>
        {
            var copy = entry.DeepClone().AsObject();
            copy["Id"] = copy["ItemId"]?.DeepClone();
            copy.Remove("ItemId");

            return JsonSerializer.Deserialize<AdminScheme>(copy);
        })];

    /// <summary>
    /// A document holding the schemes of the recipe and its default scheme, as the migration leaves it.
    /// </summary>
    public static AdminSchemesDocument CreateDocument()
    {
        var document = new AdminSchemesDocument { DefaultSchemeId = ReadStep()["DefaultSchemeId"]?.GetValue<string>() };

        foreach (var scheme in ReadSchemes())
        {
            document.Schemes[scheme.Id] = scheme;
        }

        return document;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OrchardCore.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Unable to locate the repository root from the test assembly location.");
    }
}
