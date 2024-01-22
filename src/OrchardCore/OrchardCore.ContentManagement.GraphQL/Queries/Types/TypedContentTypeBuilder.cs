using System;
using System.Collections.Concurrent;
using System.Linq;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class TypedContentTypeBuilder : IContentTypeBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GraphQLContentOptions _contentOptions;
        private readonly ConcurrentDictionary<string, Type> PartTypes = new();
        private readonly ConcurrentDictionary<Type, Type> ObjectGraphTypePartTypes = new();
        private readonly ConcurrentDictionary<Type, Type> InputObjectGraphTypePartTypes = new();
        public TypedContentTypeBuilder(IHttpContextAccessor httpContextAccessor,
            IOptions<GraphQLContentOptions> contentOptionsAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentOptions = contentOptionsAccessor.Value;
        }

        public void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var typeActivator = serviceProvider.GetService<ITypeActivatorFactory<ContentPart>>();

            if (_contentOptions.ShouldHide(contentTypeDefinition))
            {
                return;
            }

            foreach (var part in contentTypeDefinition.Parts)
            {
                if (_contentOptions.ShouldSkip(part))
                {
                    continue;
                }

                var partName = part.Name;

                // Check if another builder has already added a field for this part.
                if (contentItemType.HasField(partName))
                {
                    continue;
                }

                var partType = PartTypes.GetOrAdd(part.PartDefinition.Name, key => typeActivator.GetTypeActivator(key).Type);

                var queryGraphType = ObjectGraphTypePartTypes.GetOrAdd(partType, key =>
                {
                    return typeof(ObjectGraphType<>).MakeGenericType(key);
                });

                var collapsePart = _contentOptions.ShouldCollapse(part);

                if (serviceProvider.GetService(queryGraphType) is IObjectGraphType queryGraphTypeResolved)
                {
                    if (collapsePart)
                    {
                        foreach (var field in queryGraphTypeResolved.Fields)
                        {
                            if (_contentOptions.ShouldSkip(queryGraphType, field.Name)) continue;

                            var rolledUpField = new FieldType
                            {
                                Name = field.Name,
                                Type = field.Type,
                                Description = field.Description,
                                DeprecationReason = field.DeprecationReason,
                                Arguments = field.Arguments,
                                Resolver = new FuncFieldResolver<ContentItem, object>(context =>
                                {
                                    var nameToResolve = partName;
                                    var resolvedPart = context.Source.Get(partType, nameToResolve);

                                    return field.Resolver.ResolveAsync(new ResolveFieldContext
                                    {
                                        Arguments = context.Arguments,
                                        Source = resolvedPart,
                                        FieldDefinition = field,
                                        UserContext = context.UserContext,
                                        RequestServices = context.RequestServices
                                    });
                                })
                            };

                            contentItemType.AddField(rolledUpField);
                        }
                    }
                    else
                    {
                        var field = new FieldType
                        {
                            Name = partName.ToFieldName(),
                            Type = queryGraphTypeResolved.GetType(),
                            Description = queryGraphTypeResolved.Description,
                        };
                        contentItemType.Field(partName.ToFieldName(), queryGraphTypeResolved.GetType())
                                       .Description(queryGraphTypeResolved.Description)
                                       .Resolve(context =>
                                       {
                                           var nameToResolve = partName;
                                           var typeToResolve = context.FieldDefinition.ResolvedType.GetType().BaseType.GetGenericArguments().First();

                                           return context.Source.Get(typeToResolve, nameToResolve);
                                       });
                    }
                }

                var inputGraphType = InputObjectGraphTypePartTypes.GetOrAdd(partType, key =>
                {
                    return typeof(InputObjectGraphType<>).MakeGenericType(key);
                });
                if (serviceProvider.GetService(inputGraphType) is IInputObjectGraphType inputGraphTypeResolved)
                {
                    var whereArgument = contentQuery.Arguments.FirstOrDefault(x => x.Name == "where");
                    if (whereArgument == null)
                    {
                        return;
                    }

                    var whereInput = (ContentItemWhereInput)whereArgument.ResolvedType;

                    if (collapsePart)
                    {
                        foreach (var field in inputGraphTypeResolved.Fields)
                        {
                            whereInput.AddField(field.WithPartCollapsedMetaData().WithPartNameMetaData(partName));
                        }
                    }
                    else
                    {
                        whereInput.AddField(new FieldType
                        {
                            Type = inputGraphTypeResolved.GetType(),
                            Name = partName.ToFieldName(),
                            Description = inputGraphTypeResolved.Description
                        }.WithPartNameMetaData(partName));
                    }
                }
            }
        }
    }
}
