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
using OrchardCore.ContentManagement.Records;
using OrchardCore.RestApis.Types;
using YesSql;

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
                            var fieldType = new FieldType
                            {
                                Name = name,
                                ResolvedType = p,
                                Resolver = new FuncFieldResolver<object>(context => {
                                    var contentPartType = (Type)context.FieldDefinition.Metadata["contentPartType"];

                                    return ((ContentItem)context.Source).Get(contentPartType, contentPartType.Name);
                                }),
                                Type = p.GetType(),
                            };

                            fieldType.Metadata.Add("contentPartType", contentPart.GetType());

                            typeType.AddField(fieldType);
                        }
                    }
                }

                contentType.AddField(new FieldType
                {
                    Name = typeDefinition.Name,
                    ResolvedType = new ListGraphType(typeType),

                    Arguments = new QueryArguments(
                        new QueryArgument<IntGraphType> { Name = "id", Description = "id of the content item" },
                        new QueryArgument<BooleanGraphType> { Name = "published", Description = "is the content item published" },
                        new QueryArgument<StringGraphType> { Name = "latest", Description = "is the content item the latest version" },
                        new QueryArgument<IntGraphType> { Name = "number", Description = "version number, 1,2,3 etc" },
                        new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                        new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "same as id" },
                        new QueryArgument<StringGraphType> { Name = "contentItemIVersionId", Description = "the id of the version" }
                    ),

                    Resolver = new FuncFieldResolver<Task<IEnumerable<ContentItem>>>(async context => {
                        var contentManager = serviceProvider.GetService<IContentManager>();

                        if (context.HasPopulatedArgument("id"))
                        {
                            return new[] { await contentManager.GetAsync(context.GetArgument<string>("id")) };
                        }

                        var session = serviceProvider.GetService<ISession>();

                        var query = session.Query<ContentItem, ContentItemIndex>();

                        //foreach (var argument in context.Arguments.Where(qa => qa.Value != null))
                        //{
                        //    query = query.WithParameter(argument.Key, argument.Value);
                        //}

                        if (context.HasPopulatedArgument("published"))
                        {
                            var value = context.GetArgument<bool>("published");
                            query = query.Where(q => q.Published == value);
                        }

                        if (context.HasPopulatedArgument("latest"))
                        {
                            var value = context.GetArgument<bool>("latest");
                            query = query.Where(q => q.Latest == value);
                        }

                        if (context.HasPopulatedArgument("number"))
                        {
                            var value = context.GetArgument<int>("number");
                            query = query.Where(q => q.Number == value);
                        }

                        if (context.HasPopulatedArgument("contentType"))
                        {
                            var value = context.GetArgument<string>("contentType");
                            query = query.Where(q => q.ContentType == value);
                        }

                        if (context.HasPopulatedArgument("contentItemId"))
                        {
                            var value = context.GetArgument<string>("contentItemId");
                            query = query.Where(q => q.ContentItemId == value);
                        }

                        if (context.HasPopulatedArgument("contentItemVersionId"))
                        {
                            var value = context.GetArgument<string>("contentItemVersionId");
                            query = query.Where(q => q.ContentItemVersionId == value);
                        }

                        return await query.ListAsync();
                    })

                    //Arguments = new QueryArguments(
                    //    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "contentItemId", Description = "contentItemId of the content item" }
                    //),
                    //Resolver = new FuncFieldResolver<Task<ContentItem>>(async context => 
                    //    await serviceProvider.GetService<IContentManager>().GetAsync(context.GetArgument<string>("contentItemId")))
                });
            }
         
            Query = contentType;

            RegisterTypes(objectGraphTypes.ToArray());
        }
    }
}
