using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Testing.Recipes;

public interface IRecipeFileProvider
{
    IFileProvider FileProvider { get; }

    IEnumerable<IFileInfo> GetRecipes();
}
