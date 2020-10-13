using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Commands;
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

                    float[] coordinates = validation.Parser.ParseValue<float[]>(validation.Commands.GetValueOrDefault(ResizeWebProcessor.Xy), validation.Culture);

                    if (coordinates.Length != 2)
                    {
                        validation.Commands.Remove(ResizeWebProcessor.Xy);
                    }
                    else
                    {
                        // This prevents a 0.9999 being seen as a different image to 0.9, to prevent disk filling.
                        var x = coordinates[0].ToString("0.#");
                        var y = coordinates[1].ToString("0.#");
                        validation.Commands[ResizeWebProcessor.Xy] = x + ',' + y;
                    }

                    // Center is xy
                    // validation.Commands.Remove(ResizeWebProcessor.Xy);
                    // Anchor is position
                    //        Center = 0,
                    //
                    // Summary:
                    //     Anchors the position of the image to the top of it's bounding container.
                    // Top = 1,
                    // Bottom = 2,
                    //
                    // Summary:
                    //     Anchors the position of the image to the left of it's bounding container.
                    // Left = 3,
                    // validation.Commands.Remove(ResizeWebProcessor.Anchor);
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
