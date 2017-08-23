using System;
using System.IO;
using ImageSharp;
using ImageSharp.Web.Caching;
using ImageSharp.Web.Commands;
using ImageSharp.Web.DependencyInjection;
using ImageSharp.Web.Processors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentTypes.Editors;
using Orchard.Environment.Navigation;
using Orchard.Environment.Shell;
using Orchard.Liquid;
using Orchard.Media.Drivers;
using Orchard.Media.Fields;
using Orchard.Media.Filters;
using Orchard.Media.Models;
using Orchard.Media.Processing;
using Orchard.Media.Recipes;
using Orchard.Media.Services;
using Orchard.Media.Settings;
using Orchard.Recipes;
using Orchard.StorageProviders.FileSystem;

namespace Orchard.Media
{
    public class Startup : StartupBase
    {
        public static int[] Sizes = new[] { 16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048 };

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMediaFileStore>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

                string mediaPath = GetMediaPath(env, shellOptions.Value, shellSettings);
                return new MediaFileStore(new FileSystemStore(mediaPath, shellSettings.RequestUrlPrefix, "/media"));
            });

            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<ContentPart, ImageMediaPart>();
            services.AddMedia();

            services.AddLiquidFilter<MediaUrlFilter>("media_url");
            services.AddLiquidFilter<ResizeUrlFilter>("resize_url");
            services.AddLiquidFilter<ImageTagFilter>("img_tag");

            // ImageSharp

            services.AddImageSharpCore(
                    options =>
                    {
                        options.Configuration = Configuration.Default;
                        options.MaxBrowserCacheDays = 7;
                        options.MaxCacheDays = 365;
                        options.OnValidate = validation => 
                        {
                            // Force some parameters to prevent disk filling.
                            // For more advanced resize parameters the usage of profiles will be necessary.
                            // This can be done with a custom IImageWebProcessor implementation that would 
                            // accept profile names.

                            validation.Commands[FormatWebProcessor.Format] = "png";
                            validation.Commands.Remove(ResizeWebProcessor.Compand);
                            validation.Commands.Remove(ResizeWebProcessor.Sampler);
                            validation.Commands.Remove(ResizeWebProcessor.Xy);
                            validation.Commands.Remove(ResizeWebProcessor.Anchor);
                            validation.Commands.Remove(BackgroundColorWebProcessor.Color);

                            if (validation.Commands.Count > 0 && !validation.Commands.ContainsKey(ResizeWebProcessor.Mode))
                            {
                                validation.Commands[ResizeWebProcessor.Mode] = "max";
                            }

                            if (validation.Commands.TryGetValue(ResizeWebProcessor.Width, out var width))
                            {
                                if (Int32.TryParse(width, out var parsedWidth))
                                {
                                    if (Array.BinarySearch<int>(Sizes, parsedWidth) == -1)
                                    {
                                        validation.Commands.Clear();
                                    }
                                }
                                else
                                {
                                    validation.Commands.Remove(ResizeWebProcessor.Width);
                                }
                            }

                            if (validation.Commands.TryGetValue(ResizeWebProcessor.Height, out var height))
                            {
                                if (Int32.TryParse(height, out var parsedHeight))
                                {
                                    if (Array.BinarySearch<int>(Sizes, parsedHeight) == -1)
                                    {
                                        validation.Commands.Clear();
                                    }
                                }
                                else
                                {
                                    validation.Commands.Remove(ResizeWebProcessor.Height);
                                }
                            }
                        };
                        options.OnProcessed = _ => { };
                        options.OnPrepareResponse = _ => { };
                    })
                    .SetUriParser<QueryCollectionUriParser>()
                    .SetCache<PhysicalFileSystemCache>()
                    .AddResolver<MediaFileSystemResolver>()
                    .AddProcessor<ResizeWebProcessor>();

            // Media Field
            services.AddSingleton<ContentField, MediaField>();
            services.AddScoped<IContentFieldDisplayDriver, MediaFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MediaFieldSettingsDriver>();

            services.AddRecipeExecutionStep<MediaStep>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

            string mediaPath = GetMediaPath(env, shellOptions.Value, shellSettings);

            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }

            // ImageSharp before the static file provider
            app.UseImageSharp();

            app.UseStaticFiles(new StaticFileOptions
            {
                // The tenant's prefix is already implied by the infrastructure
                RequestPath = "/media",
                FileProvider = new PhysicalFileProvider(mediaPath)
            });
        }

        private string GetMediaPath(IHostingEnvironment env, ShellOptions shellOptions, ShellSettings shellSettings)
        {
            var relativeMediaPath = Path.Combine(shellOptions.ShellsRootContainerName, shellOptions.ShellsContainerName, shellSettings.Name, "Media");
            return env.ContentRootFileProvider.GetFileInfo(relativeMediaPath).PhysicalPath;
        }
    }
}
