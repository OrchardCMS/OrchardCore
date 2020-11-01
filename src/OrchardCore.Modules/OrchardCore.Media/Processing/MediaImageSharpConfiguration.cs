using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
            options.OnParseCommandsAsync = context =>
            {
                if (context.Commands.Count == 0)
                {
                    return Task.CompletedTask;
                }

                var mediaOptions = context.Context.RequestServices.GetRequiredService<IOptions<MediaOptions>>().Value;
                if (mediaOptions.UseTokenizedQueryString)
                {
                    if (context.Commands.TryGetValue(TokenCommandProcessor.TokenCommand, out var tokenString))
                    {
                        var mediaTokenService =  context.Context.RequestServices.GetRequiredService<IMediaTokenService>();
                        var token = context.Parser.ParseValue<string>(tokenString, context.Culture);
                        var commands = mediaTokenService.GetTokenizedCommands(token);

                        context.Commands.Clear();

                        // When token is invalid no image commands will be processed.
                        if (commands == null)
                        {
                            return Task.CompletedTask;
                        }

                        // Set commands to the tokens value.
                        foreach(var command in commands)
                        {
                            context.Commands[command.Key] = command.Value;
                        }

                        // Do not check supported sizes here, as with tokenization any size is allowed.
                    }
                    else
                    {
                        // When tokenization is enabled, but a token is not present, clear the commands to no-op the image processing pipeline.
                        context.Commands.Clear();
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    // The following commands are not supported without a tokenized query string.
                    context.Commands.Remove(ResizeWebProcessor.Xy);
                    context.Commands.Remove(ImageVersionProcessor.VersionCommand);

                    // Width and height must be part of the supported sizes array when tokenization is disabled.
                    if (context.Commands.TryGetValue(ResizeWebProcessor.Width, out var widthString))
                    {
                        var width = context.Parser.ParseValue<int>(widthString, context.Culture);

                        if (Array.BinarySearch<int>(mediaOptions.SupportedSizes, width) < 0)
                        {
                            context.Commands.Remove(ResizeWebProcessor.Width);
                        }
                    }

                    if (context.Commands.TryGetValue(ResizeWebProcessor.Height, out var heightString))
                    {
                        var height = context.Parser.ParseValue<int>(heightString, context.Culture);

                        if (Array.BinarySearch<int>(mediaOptions.SupportedSizes, height) < 0)
                        {
                            context.Commands.Remove(ResizeWebProcessor.Height);
                        }
                    }
                }

                // The following commands are not supported by default.
                context.Commands.Remove(ResizeWebProcessor.Compand);
                context.Commands.Remove(ResizeWebProcessor.Sampler);
                context.Commands.Remove(ResizeWebProcessor.Anchor);
                context.Commands.Remove(BackgroundColorWebProcessor.Color);

                // When only a version command is applied pass on this request.
                if (context.Commands.Count == 1 && context.Commands.ContainsKey(ImageVersionProcessor.VersionCommand))
                {
                    context.Commands.Clear();
                }
                else if (!context.Commands.ContainsKey(ResizeWebProcessor.Mode))
                {
                    context.Commands[ResizeWebProcessor.Mode] = "max";
                }

                return Task.CompletedTask;
            };
        }
    }
}
