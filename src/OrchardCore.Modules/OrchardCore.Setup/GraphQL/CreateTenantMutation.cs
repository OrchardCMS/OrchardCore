using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Arguments;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.ViewModels;

namespace OrchardCore.Setup.GraphQL
{
    public class CreateTenantMutation : MutationFieldType
    {
        private const string DefaultRecipe = "Default";
        public IStringLocalizer T { get; set; }

        public CreateTenantMutation(
            IHttpContextAccessor httpContextAccessor,
            ShellSettings shellSettings,
            IEnumerable<DatabaseProvider> databaseProviders,
            IStringLocalizer<CreateTenantMutation> t)
        {
            T = t;

            Name = "CreateTenant";

            Arguments = new AutoRegisteringQueryArguments<SetupViewModel>();

            Type = typeof(CreateTenantOutcomeType);

            Resolver = new AsyncFieldResolver<object, object>(async (context) =>
            {
                var model = context.MapArgumentsTo<SetupViewModel>();

                model.DatabaseProviders = databaseProviders;

                var setupService = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ISetupService>();
                model.Recipes = await setupService.GetSetupRecipesAsync();

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

                if (!String.IsNullOrEmpty(shellSettings.ConnectionString))
                {
                    model.DatabaseConfigurationPreset = true;
                    model.ConnectionString = shellSettings.ConnectionString;
                }

                if (!String.IsNullOrEmpty(shellSettings.DatabaseProvider))
                {
                    model.DatabaseConfigurationPreset = true;
                    model.DatabaseProvider = shellSettings.DatabaseProvider;
                }

                if (!String.IsNullOrEmpty(shellSettings.TablePrefix))
                {
                    model.DatabaseConfigurationPreset = true;
                    model.TablePrefix = shellSettings.TablePrefix;
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

                if (!model.DatabaseConfigurationPreset)
                {
                    setupContext.DatabaseProvider = model.DatabaseProvider;
                    setupContext.DatabaseConnectionString = model.ConnectionString;
                    setupContext.DatabaseTablePrefix = model.TablePrefix;
                }

                var executionId = await setupService.SetupAsync(setupContext);

                if (setupContext.Errors.Any())
                {
                    foreach (var error in setupContext.Errors)
                    {
                        context.Errors.Add(new ExecutionError(error.Value));
                    }

                    return null;
                }

                return new CreateTenantOutcome
                {
                    ExecutionId = executionId
                };
            });
        }
    }
}
