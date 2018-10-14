using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using System.Linq;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.ContentManagement.GraphQL.Mutations.Types
{
    public class CreateContentItemInputType : InputObjectGraphType<ContentItem>
    {
        public CreateContentItemInputType()
        {
            Field(ci => ci.ContentType, false);
            Field(ci => ci.Author, true);
            Field(ci => ci.Owner, true);

            Field(ci => ci.Published, true);
            Field(ci => ci.Latest, true);

            Field<ContentPartsInputType>("contentParts");
        }
    }

    public class ContentPartsInputType : InputObjectGraphType
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentPartsInputType(
            IHttpContextAccessor httpContextAccessor,
            IContentDefinitionManager contentDefinitionManager,
            ITypeActivatorFactory<ContentPart> partTypeActivator,
            ITypeActivatorFactory<ContentField> fieldTypeActivator,
            IEnumerable<ContentPart> cps)
        {
            Name = "ContentPartsInput";

            var requestServices = httpContextAccessor.HttpContext
                           .RequestServices;

            var existingParts = cps.Select(x => x.GetType()).ToList();

            foreach (var partDefinition in contentDefinitionManager.ListPartDefinitions())
            {
                var partName = partDefinition.Name;
                var partActivator = partTypeActivator.GetTypeActivator(partName);

                var partInputGraphType =
                    typeof(InputObjectGraphType<>).MakeGenericType(partActivator.Type);

                var partInputGraphTypeResolved = 
                    (IInputObjectGraphType)requestServices.GetService(partInputGraphType);

                if (partInputGraphTypeResolved != null)
                {
                    var arguments = new List<QueryArgument>();

                    foreach (var fieldDefinition in partDefinition.Fields)
                    {
                        var fieldName = fieldDefinition.Name;
                        var fieldActivator = fieldTypeActivator.GetTypeActivator(fieldName);

                        var fieldInputGraphType =
                            typeof(InputObjectGraphType<>).MakeGenericType(fieldActivator.Type);

                        var fieldInputGraphTypeResolved =
                            (IInputObjectGraphType)requestServices.GetService(partInputGraphType);

                        if (fieldInputGraphTypeResolved != null) {

                        }
                    }

                    Field(
                        partInputGraphTypeResolved.GetType(),
                        partName);

                    existingParts.Remove(partActivator.Type);
                }
            }

            foreach (var p in existingParts) {
                var partName = p.Name;
                var partActivator = partTypeActivator.GetTypeActivator(partName);

                var partInputGraphType =
                    typeof(InputObjectGraphType<>).MakeGenericType(partActivator.Type);

                var partInputGraphTypeResolved =
                    (IInputObjectGraphType)requestServices.GetService(partInputGraphType);

                if (partInputGraphTypeResolved != null)
                {
                    Field(
                        partInputGraphTypeResolved.GetType(),
                        partName);
                }
            }

            _httpContextAccessor = httpContextAccessor;
        }
    }
}
