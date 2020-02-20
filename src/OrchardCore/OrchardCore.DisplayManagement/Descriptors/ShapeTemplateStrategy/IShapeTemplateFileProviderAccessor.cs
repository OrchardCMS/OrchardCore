using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    /// <summary>
    /// Accessor to the <see cref="IFileProvider"/> used by <see cref="ShapeTemplateBindingStrategy"/>.
    /// </summary>
    public interface IShapeTemplateFileProviderAccessor
    {
        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Template files.
        /// </summary>
        IFileProvider FileProvider { get; }
    }
}
