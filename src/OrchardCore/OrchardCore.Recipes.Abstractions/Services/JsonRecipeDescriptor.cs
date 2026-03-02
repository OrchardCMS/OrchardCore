using System.Text;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Base class for defining recipes in code by providing JSON content.
/// </summary>
public abstract class JsonRecipeDescriptor : IRecipeDescriptor
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public virtual string Description => string.Empty;

    /// <inheritdoc />
    public virtual string Author => string.Empty;

    /// <inheritdoc />
    public virtual string WebSite => string.Empty;

    /// <inheritdoc />
    public virtual string Version => "1.0.0";

    /// <inheritdoc />
    public virtual bool IsSetupRecipe => false;

    /// <inheritdoc />
    public virtual DateTime? ExportUtc => null;

    /// <inheritdoc />
    public virtual string[] Categories => [];

    /// <inheritdoc />
    public virtual string[] Tags => [];

    /// <inheritdoc />
    public virtual bool RequireNewScope => true;

    /// <summary>
    /// Gets the full recipe JSON content.
    /// </summary>
    protected abstract string Json { get; }

    public virtual bool IsAvailable(ShellSettings shellSettings)
        => true;

    /// <inheritdoc />
    public Task<Stream> OpenReadStreamAsync()
    {
        var bytes = Encoding.UTF8.GetBytes(Json ?? string.Empty);

        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }
}
