using System;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.ViewModels;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class SetupSchema : Schema
    {
        public SetupSchema(IServiceProvider serviceProvider)
            : base(new FuncDependencyResolver((type) => (IGraphType)serviceProvider.GetService(type)))
        {
            Mutation = serviceProvider.GetService<SetupMutation>();
        }
    }

    public class SetupMutation : ObjectGraphType<object>
    {
        public SetupMutation(ISetupService setupService)
        {
            FieldAsync<SiteType>(
                "createSite",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<SetupInputType>> { Name = "site" }
                ),
                resolve: async context =>
                {
                    var human = context.GetArgument<SetupViewModel>("site");
                    return await setupService.SetupAsync(new SetupContext());
                });
        }
    }

    public class SiteType : ObjectGraphType<string>
    {

    }

    public class SetupInputType : InputObjectGraphType
    {
        public SetupInputType()
        {
            Name = "SetupInput";
            Field<StringGraphType>("siteName");
            Field<StringGraphType>("databaseProvider");
            Field<BooleanGraphType>("databaseProviderPreset");
            Field<StringGraphType>("connectionString");
            Field<BooleanGraphType>("connectionStringPreset");
            Field<StringGraphType>("tablePrefix");
            Field<StringGraphType>("tablePrefixPreset");
            Field<StringGraphType>("userName");
            Field<StringGraphType>("email");
            Field<StringGraphType>("password");
            Field<StringGraphType>("passwordConfirmation");
            Field<StringGraphType>("recipeName");
        }
    }
}
