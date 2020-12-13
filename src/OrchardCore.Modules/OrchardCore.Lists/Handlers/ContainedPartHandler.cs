using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;

namespace OrchardCore.Lists.Handlers
{
    public class ContainedPartHandler : ContentHandlerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ContainedPartHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task CloningAsync(CloneContentContext context)
        {
            var containedPart = context.CloneContentItem.As<ContainedPart>();
            if (containedPart != null)
            {
                // Resolve from DI to avoid circular references.
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                var listContentItem = await contentManager.GetAsync(containedPart.ListContentItemId);
                if (listContentItem != null)
                {
                    var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();
                    var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(listContentItem.ContentType);
                    var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "ListPart"));
                    var settings = contentTypePartDefinition.GetSettings<ListPartSettings>();
                    if (settings.EnableOrdering)
                    {
                        var containerService = _serviceProvider.GetRequiredService<IContainerService>();
                        var nextOrder = await containerService.GetNextOrderNumberAsync(containedPart.ListContentItemId);
                        context.CloneContentItem.Alter<ContainedPart>(x => x.Order = nextOrder);
                    }
                }
            }
        }
    }
}
