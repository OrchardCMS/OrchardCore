using System;
using Markdig;
using OrchardCore.Markdown.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MarkdownPipelineOptionsExtensions
    {
        /// <summary>
        /// Adds a configuration action to the markdown pipeline builder.
        /// </summary>
        public static void ConfigureMarkdownPipeline(this IServiceCollection services, Action<MarkdownPipelineBuilder> action)
        {
            services.Configure<MarkdownPipelineOptions>(o =>
            {
                o.Configure.Add(action);
            });
        }
    }
}
