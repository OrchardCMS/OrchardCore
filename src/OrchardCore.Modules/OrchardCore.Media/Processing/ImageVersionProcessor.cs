using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Media.Processing
{
    public class ImageVersionProcessor : IImageWebProcessor
    {
        private static readonly IEnumerable<string> VersionCommands = new[] { "v" };

        public IEnumerable<string> Commands => VersionCommands;

        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands)
            => image;
    }
}
