using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// Provides default configuration for ImageSharp.
    /// </summary>
    public class MediaImageSharpConfiguration : IConfigureOptions<ImageSharpMiddlewareOptions>
    {
        private readonly MediaOptions _mediaOptions;

        public MediaImageSharpConfiguration(IOptions<MediaOptions> mediaOptions)
        {
            _mediaOptions = mediaOptions.Value;
        }

        public void Configure(ImageSharpMiddlewareOptions options)
        {
            options.Configuration = Configuration.Default;
            options.BrowserMaxAge = TimeSpan.FromDays(_mediaOptions.MaxBrowserCacheDays);
            options.CacheMaxAge = TimeSpan.FromDays(_mediaOptions.MaxCacheDays);
            options.CachedNameLength = 12;
            options.OnParseCommandsAsync = validation =>
            {
                // Force some parameters to prevent disk filling.
                // For more advanced resize parameters the usage of profiles will be necessary.
                // This can be done with a custom IImageWebProcessor implementation that would
                // accept profile names.

                // The other option here is to make a query string of kitten.jpg?profile=xs?pv=guid (to break image sharp cache on change)
                // Read the command string and add it to the commands here.
                // More complicate, but with a cleaner query string.

                // https://localhost:5001/profiles/media/kitten.jpg?profile=md?pv=guid  vs
                // https://localhost:5001/profiles/media/portfolio/02-full.jpg?width=480&height=480&rmode=stretch

                if (validation.Commands.Count > 0)
                {
                    validation.Commands.Remove(ResizeWebProcessor.Compand);
                    validation.Commands.Remove(ResizeWebProcessor.Sampler);
                    validation.Commands.Remove(ResizeWebProcessor.Xy);
                    validation.Commands.Remove(ResizeWebProcessor.Anchor);
                    validation.Commands.Remove(BackgroundColorWebProcessor.Color);

                    if (!validation.Commands.ContainsKey(ResizeWebProcessor.Mode))
                    {
                        validation.Commands[ResizeWebProcessor.Mode] = "max";
                    }
                }

                return Task.CompletedTask;
            };
        }
    }
}
