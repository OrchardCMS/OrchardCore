using System;
using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.Tokens.Content.Abstractions;

namespace Orchard.Tokens
{
    public static class ContentTokens
    {
        public static void RegisterContentTokens(this IHandlebars handlebars)
        {
            // Renders a slug for the current content item. Do not use to represent the url
            // as the content item as it might be different than the computed slug.
            handlebars.RegisterHelper("slug", (output, context, arguments) =>
            {
                IServiceProvider serviceProvider = context.ServiceProvider;
                var contentManager = serviceProvider.GetRequiredService<IContentManager>();
                var slugService = serviceProvider.GetRequiredService<ISlugService>();

                ContentItem contentItem = context.Content;

                string title = contentManager.PopulateAspect<ContentItemMetadata>(contentItem).DisplayText;

                var slug = slugService.Slugify(title);
                output.Write(slug);
            });

            // The "container" block helper redefines the context.Content property to
            // the container of the current context Content property. If the content doesn't
            // have a container then the inner template is not rendered.
            // Example: {{#container}}{{slug}}/{{/container}}{{slug}}, this will render the slug of the 
            // container then the slug of the content item.
            handlebars.RegisterHelper("container", (output, options, context, arguments) =>
            {
                ContentItem contentItem = context.Content;

                string containerId = contentItem.Content?.ContainedPart?.ListContentItemId;

                if (containerId != null)
                {
                    IServiceProvider serviceProvider = context.ServiceProvider;
                    var contentManager = serviceProvider.GetRequiredService<IContentManager>();

                    var container = contentManager.GetAsync(containerId).GetAwaiter().GetResult();

                    if (container != null)
                    {
                        var previousContent = context.Content;
                        context.Content = container;
                        options.Template(output, context);
                        context.Content = previousContent;
                    }
                }
            });
        }
    }
}
