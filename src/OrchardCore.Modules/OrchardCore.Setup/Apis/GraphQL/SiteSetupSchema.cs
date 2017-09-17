using System;
using Microsoft.Extensions.DependencyInjection;
using GraphQL;
using GraphQL.Types;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class SiteSetupSchema : Schema
    {
        public SiteSetupSchema(IServiceProvider serviceProvider)
            : base(new FuncDependencyResolver((type) => (IGraphType)serviceProvider.GetService(type)))
        {
            Query = serviceProvider.GetService<SiteQuery>();
            Mutation = serviceProvider.GetService<SiteSetupMutation>();
        }
    }

    public class SiteQuery : ObjectGraphType<object>
    {
        public SiteQuery()
        {
            Name = "Query";
        }
    }

    public class SiteSetupMutation : ObjectGraphType<object>
    {
        public SiteSetupMutation(ISetupService setupService)
        {
            Name = "Mutation";

            FieldAsync<SiteSetupOutcomeType>(
                "createSite",
                arguments: new QueryArguments(
                    new QueryArgument<SiteSetupInputType> { Name = "site" }
                ),
                resolve: async context =>
                {
                    var site = context.GetArgument<SetupViewModel>("site");

                    var setupContext = new SetupContext
                    {
                        SiteName = site.SiteName,
                        EnabledFeatures = null, // default list,
                        AdminUsername = site.UserName,
                        AdminEmail = site.Email,
                        AdminPassword = site.Password,
                        Errors = new Dictionary<string, string>(),
                        Recipe = (await setupService.GetSetupRecipesAsync()).FirstOrDefault(rd => rd.RecipeFileInfo.Name == site.RecipeName),
                        DatabaseProvider = site.DatabaseProvider
                    };

                    return new SiteSetupOutcome {
                        ExecutionId = await setupService.SetupAsync(setupContext)
                    };
                });
        }
    }

    public class SiteSetupOutcome {
        public string ExecutionId { get; set; }
    }

    public class SiteSetupOutcomeType : AutoRegisteringObjectGraphType<SiteSetupOutcome>
    {
        public SiteSetupOutcomeType()
        {
            Name = "Site";
        }
    }

    public class SiteSetupInputType : InputObjectGraphType
    {
        public SiteSetupInputType()
        {
            Name = "SiteSetupInput";

            Field<StringGraphType>("siteName");
            Field<StringGraphType>("databaseProvider");
            //Field<BooleanGraphType>("databaseProviderPreset");
            //Field<StringGraphType>("connectionString");
            //Field<BooleanGraphType>("connectionStringPreset");
            //Field<StringGraphType>("tablePrefix");
            //Field<StringGraphType>("tablePrefixPreset");
            Field<StringGraphType>("userName");
            Field<StringGraphType>("email");
            Field<StringGraphType>("password");
            Field<StringGraphType>("passwordConfirmation");
            Field<StringGraphType>("recipeName");
        }
    }
}
