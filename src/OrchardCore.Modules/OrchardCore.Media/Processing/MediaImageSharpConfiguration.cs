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
            options.OnParseCommandsAsync = commandContext =>
            {
                if (commandContext.Commands.Count == 0)
                {
                    return Task.CompletedTask;
                }

                var mediaOptions = commandContext.Context.RequestServices.GetRequiredService<IOptions<MediaOptions>>().Value;
                if (mediaOptions.UseTokenizedQueryString)
                {
                    if (commandContext.Commands.ContainsKey(TokenCommandProcessor.TokenCommand))
                    {
                        var mediaTokenService =  commandContext.Context.RequestServices.GetRequiredService<IMediaTokenService>();
                        var token = commandContext.Parser.ParseValue<string>(commandContext.Commands.GetValueOrDefault(TokenCommandProcessor.TokenCommand), commandContext.Culture);
                        var commands = mediaTokenService.GetTokenizedCommands(token);

                        commandContext.Commands.Clear();

                        // When token is invalid no image commands will be processed.
                        if (commands == null)
                        {
                            return Task.CompletedTask;
                        }

                        // Set commands to the tokens value.
                        foreach(var command in commands)
                        {
                            commandContext.Commands[command.Key] = command.Value;
                        }
                    }
                    else
                    {
                        // When tokenization is enabled, but a token is not present, clear the commands to no-op the image processing pipeline.
                        commandContext.Commands.Clear();
                        return Task.CompletedTask;
                    }
                }

                commandContext.Commands.Remove(ResizeWebProcessor.Compand);
                commandContext.Commands.Remove(ResizeWebProcessor.Sampler);
                if (commandContext.Commands.ContainsKey(ResizeWebProcessor.Xy))
                {
                    var coordinates = commandContext.Parser.ParseValue<float[]>(commandContext.Commands.GetValueOrDefault(ResizeWebProcessor.Xy), commandContext.Culture);

                    if (coordinates.Length != 2)
                    {
                        commandContext.Commands.Remove(ResizeWebProcessor.Xy);
                    }
                    else if (!mediaOptions.UseTokenizedQueryString)
                    {
                        var cropFormat = "0." + new String('#', mediaOptions.CropPrecision);
                        var x = coordinates[0].ToString(cropFormat);
                        var y = coordinates[1].ToString(cropFormat);
                        commandContext.Commands[ResizeWebProcessor.Xy] = x + ',' + y;
                    }
                }

                commandContext.Commands.Remove(ResizeWebProcessor.Anchor);
                commandContext.Commands.Remove(BackgroundColorWebProcessor.Color);

                if (!commandContext.Commands.ContainsKey(ResizeWebProcessor.Mode))
                {
                    commandContext.Commands[ResizeWebProcessor.Mode] = "max";
                }

                return Task.CompletedTask;
            };
        }
    }
}
