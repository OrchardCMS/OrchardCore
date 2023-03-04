using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using OrchardCore.Testing.Recipes;

namespace OrchardCore.Tests.Apis.Context;

public class RecipeFileProvider : IRecipeFileProvider
{
    public IFileProvider FileProvider => new EmbeddedFileProvider(GetType().GetTypeInfo().Assembly);

    public IEnumerable<IFileInfo> GetRecipes()
    {
        yield return FileProvider.GetFileInfo("Apis/Lucene/Recipes/luceneQueryTest.json");
    }
}
