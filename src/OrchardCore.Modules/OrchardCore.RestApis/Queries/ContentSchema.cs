using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.RestApis.Types;

namespace OrchardCore.RestApis.Queries
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<ContentPart> contentParts,
            IEnumerable<IObjectGraphType> objectGraphTypes)
            : base(new FuncDependencyResolver((type) => (IGraphType)serviceProvider.GetService(type)))
        {
            Mutation = serviceProvider.GetService<ContentItemMutation>();

            var contentType = serviceProvider.GetService<ContentType>();

            var typeDefinitions = contentDefinitionManager.ListTypeDefinitions();

            foreach (var typeDefinition in typeDefinitions)
            {
                var typeType = new ContentItemType
                {
                    Name = typeDefinition.Name, // Blog
                };

                foreach (var part in typeDefinition.Parts)
                {
                    var name = part.Name; // About
                    var partName = part.PartDefinition.Name; // BagPart

                    var contentPart = contentParts.FirstOrDefault(x => x.GetType().Name == partName);

                    if (contentPart != null)
                    {
                        var p = objectGraphTypes.FirstOrDefault(x => x.IsTypeOf(contentPart));

                        if (p != null)
                        {
                            // Add Field needs to be like Content Item and Content Items.... 
                            // so you can filter by blog...
                            typeType.AddField(new FieldType
                            {
                                Name = name,
                                ResolvedType = p,
                                Resolver = new FuncFieldResolver<ContentItemType, Task<object>>(context => null),
                                Type = p.GetType()
                            });
                        }
                    }
                }

                contentType.AddField(new FieldType
                {
                    Name = typeDefinition.Name,
                    ResolvedType = typeType,
                    Arguments = new QueryArguments(
                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "contentItemId", Description = "contentItemId of the content item" }
                    ),
                    Resolver = new FuncFieldResolver<Task<ContentItem>>(context => serviceProvider.GetService<IContentManager>().GetAsync(context.GetArgument<string>("contentItemId")))
                });
            }
         
            ////AddField(new EventStreamFieldType
            ////{
            ////    Name = "messageAdded",
            ////    Type = typeof(MessageType),
            ////    Resolver = new EventStreamResolver(Subscribe)
            ////});

            Query = contentType;

            RegisterTypes(objectGraphTypes.ToArray());
        }
    }
}
