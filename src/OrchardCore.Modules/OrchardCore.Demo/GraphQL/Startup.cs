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
                        new GraphQLContentTypeOption {
                            ContentType = "Blog",
                            Collapse = false,
                            Ignore = false,
                            PartOptions = new GraphQLContentPartOption[] {
                                // Content Part options attached to Content Type
                                new GraphQLContentPartOption {
                                    Name = "TestContentPartA",
                                    Collapse = false,
                                    Ignore = false
                                },
                                new GraphQLContentPartOption<TestContentPartA> {
                                    Collapse = false,
                                    Ignore = false
                                }
                            }
                        }
                    }
                );

                options.ConfigureContentType("Blog", (typeConfig) =>
                {
                    typeConfig.Collapse = false;
                    typeConfig.Ignore = false;

                    typeConfig
                        .ConfigurePart("TestContentPartA", (partConfig) =>
                        {
                            partConfig.Collapse = false;
                            partConfig.Ignore = false;
                        })
                        .ConfigurePart<TestContentPartA>((partConfig) =>
                        {
                            partConfig.Collapse = false;
                            partConfig.Ignore = false;
                        });
                });

                // Ignore Fields on GraphQL Objects
                options.IgnoredFields = options.IgnoredFields.Union(new[] {
                    new GraphQLField(typeof(TestQueryObjectType), "lineIgnored"),
                    new GraphQLField<TestQueryObjectType>("lineOtherIgnored")
                });

                options
                    .IgnoreField(typeof(TestQueryObjectType), "lineIgnored")
                    .IgnoreField<TestQueryObjectType>("lineIgnored");

                // Top level Part Options
                options.PartOptions = options.PartOptions.Union(new[] {
                    new GraphQLContentPartOption {
                        Name = "TestContentPartA",
                        Collapse = false,
                        Ignore = false
                    },
                    new GraphQLContentPartOption<TestContentPartA> {
                        Collapse = false,
                        Ignore = false
                    }
                });

                options
                    .ConfigurePart("TestContentPartA", (partConfig) =>
                    {
                        partConfig.Collapse = false;
                        partConfig.Ignore = false;
                    })
                    .ConfigurePart<TestContentPartA>((partConfig) =>
                    {
                        partConfig.Collapse = false;
                        partConfig.Ignore = false;
                    });
            });
        }
    }
}
