using System;
using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;

namespace Orchard.Tokens
{
    public static class ContentTokens
    {
        public static void RegisterContentTokens(this IHandlebars handlebars)
        {
            handlebars.RegisterHelper("slug", (output, context, arguments) =>
            {
                IServiceProvider serviceProvider = context.ServiceProvider;
                var contentManager = serviceProvider.GetRequiredService<IContentManager>();

                ContentItem contentItem = context.Content;

                string title = contentManager.PopulateAspect<ContentItemMetadata>(contentItem).DisplayText;

                var slug = title?.ToLower().Replace(" ", "-");
                output.Write(slug);
            });
        }
    }
}
