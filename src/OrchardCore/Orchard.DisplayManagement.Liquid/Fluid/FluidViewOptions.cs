using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace Orchard.DisplayManagement.Fluid
{
    /// <summary>
    /// Provides programmatic configuration for the <see cref="FluidViewTemplate"/>.
    /// </summary>
    public class FluidViewOptions
    {
        /// <summary>
        /// Gets the sequence of <see cref="IFileProvider" /> instances used by <see cref="FluidViewTemplate"/> to
        /// locate Fluid files.
        /// </summary>
        /// <remarks>
        /// At startup, this is initialized to include an instance of <see cref="PhysicalFileProvider"/> that is
        /// rooted at the application root.
        /// </remarks>
        public IList<IFileProvider> FileProviders { get; } = new List<IFileProvider>();
    }
}