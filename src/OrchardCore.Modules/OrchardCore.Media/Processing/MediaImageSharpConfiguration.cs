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
