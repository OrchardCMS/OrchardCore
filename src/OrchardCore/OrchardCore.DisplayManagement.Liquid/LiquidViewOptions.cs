using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// Provides programmatic configuration for the <see cref="LiquidViewTemplate"/>.
    /// </summary>
    public class LiquidViewOptions
    {
        /// <summary>
        /// Gets the sequence of <see cref="IFileProvider" /> instances used by <see cref="LiquidViewTemplate"/> to
        /// locate Liquid files.
        /// </summary>
        /// <remarks>
        /// At startup, this is initialized to include an instance of <see cref="PhysicalFileProvider"/> that is
        /// rooted at the application root.
        /// </remarks>
        public List<IFileProvider> FileProviders { get; } = new List<IFileProvider>();
        public List<Action<LiquidViewParser>> LiquidViewParserConfiguration { get; } = new List<Action<LiquidViewParser>>();
    }
}
