using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.Demo.Models;
using OrchardCore.Modules;

namespace OrchardCore.Demo.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<GraphQLContentOptions>(options =>
            {
                // Top Level Content Type options
                options.ContentTypeOptions = options.ContentTypeOptions.Union(new[] {
                        new GraphQLContentTypeOption("Blog")
                        {
                            Collapse = false,
                            Hidden = false,
                            PartOptions = new GraphQLContentPartOption[] {
                                // Content Part options attached to Content Type
                                new GraphQLContentPartOption("TestContentPartA")
                                {
                                    Collapse = false,
                                    Hidden = false
                                },
                                new GraphQLContentPartOption<TestContentPartA>
                                {
                                    Collapse = false,
                                    Hidden = false
                                }
                            }
                        }
                    }
                );

                options.ConfigureContentType("Blog", (typeConfig) =>
                {
                    typeConfig.Collapse = false;
                    typeConfig.Hidden = false;

                    typeConfig
                        .ConfigurePart("TestContentPartA", (partConfig) =>
                        {
                            partConfig.Collapse = false;
                            partConfig.Hidden = false;
                        })
                        .ConfigurePart<TestContentPartA>((partConfig) =>
                        {
                            partConfig.Collapse = false;
                            partConfig.Hidden = false;
                        });
                });

                // Ignore Fields on GraphQL Objects
                options.HiddenFields = options.HiddenFields.Union(new[] {
                    new GraphQLField(typeof(TestQueryObjectType), "lineIgnored"),
                    new GraphQLField<TestQueryObjectType>("lineOtherIgnored")
                });

                options
                    .IgnoreField(typeof(TestQueryObjectType), "lineIgnored")
                    .IgnoreField<TestQueryObjectType>("lineIgnored");

                // Top level Part Options
                options.PartOptions = options.PartOptions.Union(new[] {
                    new GraphQLContentPartOption("TestContentPartA")
                    {
                        Collapse = false,
                        Hidden = false
                    },
                    new GraphQLContentPartOption<TestContentPartA>
                    {
                        Collapse = false,
                        Hidden = false
                    }
                });

                options
                    .ConfigurePart("TestContentPartA", (partConfig) =>
                    {
                        partConfig.Collapse = false;
                        partConfig.Hidden = false;
                    })
                    .ConfigurePart<TestContentPartA>((partConfig) =>
                    {
                        partConfig.Collapse = false;
                        partConfig.Hidden = false;
                    });
            });
        }
    }
}
