using System;
using Microsoft.Extensions.DependencyInjection;
using GraphQL;
using GraphQL.Types;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Data;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class SiteSetupSchema : Schema
    {
        public SiteSetupSchema(IServiceProvider serviceProvider)
            : base(new FuncDependencyResolver((type) => (IGraphType)serviceProvider.GetService(type)))
        {
            Mutation = serviceProvider.GetService<SiteSetupMutation>();
        }
    }

    public class SiteSetupMutation : ObjectGraphType<object>
    {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;

        public IStringLocalizer T { get; set; }

        public SiteSetupMutation(ISetupService setupService,
            ShellSettings shellSettings,
            IEnumerable<DatabaseProvider> databaseProviders,
            IStringLocalizer<SiteSetupMutation> t)
        {
            _setupService = setupService;
            _shellSettings = shellSettings;
            _databaseProviders = databaseProviders;

            T = t;

            Name = "Mutation";

            FieldAsync<SiteSetupOutcomeType>(
                "createSite",
                arguments: new QueryArguments(
                    new QueryArgument<SiteSetupInputType> { Name = "site" }
                ),
                resolve: async context =>
                {
                    var model = context.GetArgument<SetupViewModel>("site");

                    model.DatabaseProviders = _databaseProviders;
                    model.Recipes = await _setupService.GetSetupRecipesAsync();

                    var selectedProvider = model.DatabaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);

                    if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(model.ConnectionString))
                    {
                        context.Errors.Add(new ExecutionError(T["The connection string is mandatory for this provider."]));
                    }

                    if (String.IsNullOrEmpty(model.Password))
                    {
                        context.Errors.Add(new ExecutionError(T["The password is required."]));
                    }

                    if (model.Password != model.PasswordConfirmation)
                    {
                        context.Errors.Add(new ExecutionError(T["The password confirmation doesn't match the password."]));
                    }

                    RecipeDescriptor selectedRecipe = null;

                    if (String.IsNullOrEmpty(model.RecipeName) || (selectedRecipe = model.Recipes.FirstOrDefault(x => x.Name == model.RecipeName)) == null)
                    {
                        context.Errors.Add(new ExecutionError(T["Invalid recipe."]));
                    }

                    if (!String.IsNullOrEmpty(_shellSettings.ConnectionString))
                    {
                        model.ConnectionStringPreset = true;
                        model.ConnectionString = _shellSettings.ConnectionString;
                    }

                    if (!String.IsNullOrEmpty(_shellSettings.DatabaseProvider))
                    {
                        model.DatabaseProviderPreset = true;
                        model.DatabaseProvider = _shellSettings.DatabaseProvider;
                    }

                    if (!String.IsNullOrEmpty(_shellSettings.TablePrefix))
                    {
                        model.TablePrefixPreset = true;
                        model.TablePrefix = _shellSettings.TablePrefix;
                    }

                    if (context.Errors.Count > 0)
                    {
                        return null;
                    }

                    var setupContext = new SetupContext
                    {
                        SiteName = model.SiteName,
                        EnabledFeatures = null, // default list,
                        AdminUsername = model.UserName,
                        AdminEmail = model.Email,
                        AdminPassword = model.Password,
                        Errors = new Dictionary<string, string>(),
                        Recipe = selectedRecipe
                    };

                    if (!model.DatabaseProviderPreset)
                    {
                        setupContext.DatabaseProvider = model.DatabaseProvider;
                    }

                    if (!model.ConnectionStringPreset)
                    {
                        setupContext.DatabaseConnectionString = model.ConnectionString;
                    }

                    if (!model.TablePrefixPreset)
                    {
                        setupContext.DatabaseTablePrefix = model.TablePrefix;
                    }

                    var executionId = await _setupService.SetupAsync(setupContext);

                    if (setupContext.Errors.Any())
                    {
                        foreach (var error in setupContext.Errors)
                        {
                            context.Errors.Add(new ExecutionError(error.Value));
                        }

                        return null;
                    }

                    return new SiteSetupOutcome {
                        ExecutionId = executionId
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
