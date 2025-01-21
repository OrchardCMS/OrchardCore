using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL;

public class ContainedQueryObjectType : ObjectGraphType<ContainedPart>
{
    public ContainedQueryObjectType(IStringLocalizer<ContainedQueryObjectType> S)
    {
        Name = nameof(ContainedPart);
        Description = S["Represents a link to the parent content item and the order in which the current content item is represented."];

        Field(x => x.ListContentItemId);

        Field<ContentItemInterface, ContentItem>("listContentItem")
           .Description(S["the parent list content item"])
           .ResolveLockedAsync(x =>
           {
               var contentItemId = x.Source.ListContentItemId;
               var contentManager = x.RequestServices.GetService<IContentManager>();

               return contentManager.GetAsync(contentItemId);
           });

        Field(x => x.ListContentType)
            .Description(S["the content type of the list owning the current content item."]);

        Field(x => x.Order)
            .Description(S["the order of the current content item in the list."]);
    }
}
