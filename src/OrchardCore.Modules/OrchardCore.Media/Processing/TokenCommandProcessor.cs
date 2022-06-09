using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// Pass through processor which allows inclusion of a tokenized query string.
    /// </summary>
    public class TokenCommandProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for a tokenized query string.
        /// </summary>
        public const string TokenCommand = "token";

        private static readonly IEnumerable<string> TokenCommands = new[] { TokenCommand };

        public IEnumerable<string> Commands
            => TokenCommands;

        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
            => image;

        public FormattedImage Process(FormattedImage image, ILogger logger, CommandCollection commands, CommandParser parser, CultureInfo culture)
            => image;

        public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture)
            => false;
    }
}
