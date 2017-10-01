using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
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

            //var typeDefinitions = contentDefinitionManager.ListTypeDefinitions();

            //foreach (var typeDefinition in typeDefinitions)
            //{
            //    var typeType = new ObjectGraphType
            //    {
            //        Name = typeDefinition.Name
            //    };

            //    foreach (var part in typeDefinition.Parts)
            //    {
            //        var name = part.Name; // About
            //        var partName = part.PartDefinition.Name; // BagPart

            //        var contentPart = contentParts.FirstOrDefault(x => x.GetType().Name == partName);

            //        if (contentPart != null)
            //        {
            //            var p = objectGraphTypes.FirstOrDefault(x => x.IsTypeOf(contentPart));

            //            if (p != null)
            //            {
            //                typeType.AddField(new FieldType
            //                {
            //                    //Type = contentPart.GetType(),
            //                    Name = name,
            //                    ResolvedType = (IObjectGraphType)serviceProvider.GetService(p.GetType())
            //                });
            //            }
            //        }
            //    }

            //    contentType.AddField(new FieldType
            //    {
            //        Name = typeDefinition.Name,
            //        ResolvedType = typeType
            //    });
            //}
            ////AddField(new EventStreamFieldType
            ////{
            ////    Name = "messageAdded",
            ////    Type = typeof(MessageType),
            ////    Resolver = new EventStreamResolver(Subscribe)
            ////});

            Query = contentType;

            //RegisterType<TitlePartType>();
            //RegisterType<AutoRoutePartType>();
            //RegisterType<BagPartType>();
        }
    }
    
    public class ContentItemMutation : ObjectGraphType<object>
    {
        public ContentItemMutation(IContentManager contentManager,
            IContentItemDisplayManager contentDisplay)
        {
            Name = "Mutation";

            FieldAsync<ContentItemType>(
                "createContentItem",
                arguments: new QueryArguments(
                    new QueryArgument<ContentItemInputType> { Name = "contentItem" }
                ),
                resolve: async context =>
                {
                    var contentItemFabrication = context.GetArgument<ContentItem>("contentItem");

                    var contentParts = JObject.Parse(
                        (context.Arguments["contentItem"] as IDictionary<string, object>)["contentParts"].ToString());

                    var contentItem = contentManager.New(contentItemFabrication.ContentType);

                    contentItem.Author = contentItemFabrication.Author;
                    contentItem.Owner = contentItemFabrication.Owner;

                    var updateModel = new ApiUpdateModel(null, contentParts);

                    await contentDisplay.UpdateEditorAsync(contentItem, updateModel);

                    if (contentItemFabrication.Published)
                    {
                        await contentManager.PublishAsync(contentItem);
                    }
                    else
                    {
                        contentManager.Create(contentItem);
                    }

                    return contentItem;
                });
        }
    }

    public class ApiUpdateModel : IUpdateModel
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly JObject _model;

        public ModelStateDictionary ModelState => new ModelStateDictionary();

        public ApiUpdateModel(IModelMetadataProvider metadataProvider,  JObject model) {
            _metadataProvider = metadataProvider;
            _model = model;
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        {
            return Task.FromResult(false);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        {
            return Task.FromResult(false);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        {
            var expression = ModelBindingHelper.GetPropertyFilterExpression(includeExpressions);
            var propertyFilter = expression.Compile();

            var modelMetadata = _metadataProvider.GetMetadataForType(model.GetType());
        
            

            return Task.FromResult(false);
        }

        public bool TryValidateModel(object model)
        {
            return false;
        }

        public bool TryValidateModel(object model, string prefix)
        {
            return false;
        }
    }

    public class ContentItemInputType : InputObjectGraphType
    {
        public ContentItemInputType(IContentDefinitionManager contentDefinitionManager)
        {
            Name = "ContentItemInput";

            Field<StringGraphType>("contentType");
            Field<StringGraphType>("owner");
            Field<StringGraphType>("author");

            Field<BooleanGraphType>("published");

            Field<StringGraphType>("contentParts");





            //var part = new ContentPart();

            //foreach (var partDefinition in contentDefinitionManager.ListPartDefinitions())
            //{
            //    var partName = partDefinition.Name; // BagPart
            //}


            //AddField(new FieldType() {
            //    Name = "titlePart",
            //    ResolvedType = new AutoRegisteringInputObjectGraphType("titlePart", part.GetType()) });
        }
    }

    public class AutoRegisteringInputObjectGraphType : InputObjectGraphType
    {
        public AutoRegisteringInputObjectGraphType(string name, Type type)
        {
            Name = name;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string));

            foreach (var propertyInfo in properties)
            {
                Field(propertyInfo.PropertyType.GetGraphTypeFromType(propertyInfo.PropertyType.IsNullable()), propertyInfo.Name);
            }
        }
    }
}
