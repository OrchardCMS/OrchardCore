using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using YesSql;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class DynamicQueryFieldTypeProvider : IDynamicQueryFieldTypeProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<ContentPart> _contentParts;
        private readonly IEnumerable<IObjectGraphType> _objectGraphTypes;
        private readonly ISession _session;

        public DynamicQueryFieldTypeProvider(
         IServiceProvider serviceProvider,
         IContentManager contentManager,
         IContentDefinitionManager contentDefinitionManager,
         IEnumerable<ContentPart> contentParts,
         IEnumerable<IObjectGraphType> objectGraphTypes,
         ISession session)
        {
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentParts = contentParts;
            _objectGraphTypes = objectGraphTypes;
            _session = session;
        }

        public IEnumerable<FieldType> GetFields()
        {
            var fieldTypes = new List<FieldType>();

            var typeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

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

                    var contentPart = _contentParts.FirstOrDefault(x => x.GetType().Name == partName);

                    if (contentPart != null)
                    {
                        var p = _objectGraphTypes.FirstOrDefault(x => x.IsTypeOf(contentPart));

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

                fieldTypes.Add(new FieldType
                {
                    Name = typeDefinition.Name,
                    ResolvedType = new ListGraphType(typeType),

                    Arguments = new QueryArguments(
                        new QueryArgument<IntGraphType> { Name = "id", Description = "id of the content item" },
                        new QueryArgument<BooleanGraphType> { Name = "published", Description = "is the content item published", DefaultValue = true },
                        new QueryArgument<StringGraphType> { Name = "latest", Description = "is the content item the latest version", DefaultValue = true },
                        new QueryArgument<IntGraphType> { Name = "number", Description = "version number, 1,2,3 etc" },
                        new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                        new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "same as id" },
                        new QueryArgument<StringGraphType> { Name = "contentItemVersionId", Description = "the id of the version" }
                    ),

                    Resolver = new FuncFieldResolver<Task<IEnumerable<ContentItem>>>(async context => {
                        if (context.HasPopulatedArgument("contentItemId"))
                        {
                            return new[] { await _contentManager.GetAsync(context.GetArgument<string>("contentItemId")) };
                        }

                        var isPublished = context.GetArgument<bool>("published");
                        var isLatest = context.GetArgument<bool>("latest");

                        var query = _session.Query<ContentItem, ContentItemIndex>().Where(q =>
                            q.Published == isPublished &&
                            q.Latest == isLatest);

                        if (context.HasPopulatedArgument("id"))
                        {
                            var value = context.GetArgument<int>("id");
                            query = query.Where(q => q.Id == value);
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
                        else
                        {
                            var value = (context.ReturnType as ListGraphType).ResolvedType.Name;
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
                });
            }

            return fieldTypes;
        }
    }
}
