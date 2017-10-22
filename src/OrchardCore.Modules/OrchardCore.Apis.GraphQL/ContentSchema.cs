using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Apis.GraphQL
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider,
            IEnumerable<ContentPart> contentParts,
            IDependencyResolver dependencyResolver)
            : base(dependencyResolver)
        {
            Mutation = serviceProvider.GetService<MutationsSchema>();
            Query = serviceProvider.GetService<QueriesSchema>();

            RegisterTypes(
                contentParts.Select(
                    contentPart => new ContentPartAutoRegisteringObjectGraphType(contentPart)).ToArray());
        }
    }
}
