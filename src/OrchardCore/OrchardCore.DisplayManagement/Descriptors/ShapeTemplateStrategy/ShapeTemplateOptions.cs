using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    /// <summary>
    /// Provides programmatic configuration for the <see cref="ShapeTemplateBindingStrategy"/>.
    /// </summary>
    public class ShapeTemplateOptions
    {
        /// <summary>
        /// Gets the sequence of <see cref="IFileProvider" /> instances used by <see cref="ShapeTemplateBindingStrategy"/> to
        /// locate Fluid files.
        /// </summary>
        /// <remarks>
        /// At startup, this is initialized to include an instance of <see cref="PhysicalFileProvider"/> that is
        /// rooted at the application root.
        /// </remarks>
        public IList<IFileProvider> FileProviders { get; } = new List<IFileProvider>();
    }
}
