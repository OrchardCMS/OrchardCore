using Microsoft.Extensions.FileProviders;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
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

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Template files across all tenants.
        /// </summary>
        IFileProvider SharedFileProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFileProvider"/> used to look up Template files for a given tenant.
        /// </summary>
        IFileProvider ShellFileProvider { get; }
    }
}