using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// Pass through processor which allows inclusion of a version query string in the cache key.
    /// </summary>
    public class ImageVersionProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for a version query string.
        /// </summary>
        public const string VersionCommand = "v";

        private static readonly IEnumerable<string> _versionCommands = new[] { VersionCommand };

        public IEnumerable<string> Commands => _versionCommands;

        public FormattedImage Process(FormattedImage image, ILogger logger, CommandCollection commands, CommandParser parser, CultureInfo culture)
            => image;

        public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture)
            => false;
    }
}
