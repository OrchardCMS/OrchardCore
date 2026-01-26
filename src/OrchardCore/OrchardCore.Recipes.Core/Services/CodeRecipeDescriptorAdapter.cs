using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// An adapter that wraps an <see cref="IRecipeDescriptor"/> instance to provide
/// backward compatibility with the existing <see cref="RecipeDescriptor"/>-based infrastructure.
/// </summary>
internal sealed class CodeRecipeDescriptorAdapter : RecipeDescriptor
{
    private readonly IRecipeDescriptor _descriptor;
    private IFileInfo _fileInfo;

    public CodeRecipeDescriptorAdapter(IRecipeDescriptor descriptor)
    {
        _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        // Copy properties from the IRecipeDescriptor.
        Name = descriptor.Name;
        DisplayName = descriptor.DisplayName;
        Description = descriptor.Description;
        Author = descriptor.Author;
        WebSite = descriptor.WebSite;
        Version = descriptor.Version;
        IsSetupRecipe = descriptor.IsSetupRecipe;
        ExportUtc = descriptor.ExportUtc;
        Categories = descriptor.Categories;
        Tags = descriptor.Tags;
        RequireNewScope = descriptor.RequireNewScope;

        // Set up a virtual file provider for code-based recipes.
        FileProvider = new CodeRecipeFileProvider(descriptor);
        BasePath = string.Empty;
    }

    /// <summary>
    /// Gets the underlying <see cref="IRecipeDescriptor"/> instance.
    /// </summary>
    public IRecipeDescriptor UnderlyingDescriptor => _descriptor;

    /// <summary>
    /// Gets the recipe file info. This is lazily created on first access.
    /// </summary>
    public new IFileInfo RecipeFileInfo
    {
        get
        {
            _fileInfo ??= new CodeRecipeFileInfo(_descriptor);
            base.RecipeFileInfo = _fileInfo;
            return _fileInfo;
        }
        set => base.RecipeFileInfo = value;
    }

    /// <summary>
    /// A virtual file provider for code-based recipes.
    /// </summary>
    private sealed class CodeRecipeFileProvider : IFileProvider
    {
        private readonly IRecipeDescriptor _descriptor;

        public CodeRecipeFileProvider(IRecipeDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath) || subpath == $"{_descriptor.Name}.recipe.json")
            {
                return new CodeRecipeFileInfo(_descriptor);
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
            => NullChangeToken.Singleton;
    }

    /// <summary>
    /// A virtual file info for code-based recipes.
    /// </summary>
    private sealed class CodeRecipeFileInfo : IFileInfo
    {
        private readonly IRecipeDescriptor _descriptor;

        public CodeRecipeFileInfo(IRecipeDescriptor descriptor)
        {
            _descriptor = descriptor;
            Name = $"{descriptor.Name}.recipe.json";
        }

        public bool Exists => true;

        public long Length => -1;

        public string PhysicalPath => null;

        public string Name { get; }

        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            // Block on the async method for compatibility with IFileInfo.
            // This is acceptable because the code-based recipe content is generated in memory.
            return _descriptor.OpenReadStreamAsync().GetAwaiter().GetResult();
        }
    }
}
