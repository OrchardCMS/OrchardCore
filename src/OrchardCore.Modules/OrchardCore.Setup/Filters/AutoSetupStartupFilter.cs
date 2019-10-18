using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Setup.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Environment.Shell;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using OrchardCore.Setup.Options;
using YesSql;
using System.Data.Common;
using System.Data;

namespace OrchardCore.Setup
{
    public class AutoSetupStartupFilter : IStartupFilter
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;
        private readonly IStringLocalizer T;

        public AutoSetupStartupFilter(IServiceProvider provider, IStringLocalizer<AutoSetupStartupFilter> stringLocalizer, ILogger<AutoSetupStartupFilter> logger)
        {
            T = stringLocalizer;
            _provider = provider;
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            using (var scope = _provider.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetRequiredService<IShellSettingsManager>();
                var siteIsUninitialized = shellService
                    .LoadSettings()
                    .All(s => s.State == OrchardCore.Environment.Shell.Models.TenantState.Uninitialized);

                if (siteIsUninitialized)
                {
                    var optionsAccessor = scope.ServiceProvider.GetRequiredService<IOptions<AutoSetupOptions>>();
                    var options = optionsAccessor.Value;

                    if (options != null)
                    {
                        _logger.LogInformation(T["AutoSetup is initializing the site"]);
                        var validationContext = new ValidationContext(options, _provider, null);
                        var validationErrors = options.Validate(validationContext);

                        if (validationErrors.Any())
                        {
                            var stringBuilder = new StringBuilder();
                            foreach (var error in validationErrors)
                            {
                                stringBuilder.Append(error.ErrorMessage);
                            }
                            _logger.LogError(T["AutoSetup did not start, configuration has following errors: {0}", stringBuilder.ToString()]);
                        }
                        else
                        {
                            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                            var setupService = scope.ServiceProvider.GetRequiredService<ISetupService>();

                            var setupContext = GetSetupContext(options, setupService, shellSettings);

                            if (options.CreateDatabase)
                                CreateDatabase(scope, options, setupContext);

                            setupService.SetupAsync(setupContext)
                                .GetAwaiter()
                                .GetResult();

                            if (setupContext.Errors.Count == 0)
                            {
                                _logger.LogInformation(T["AutoSetup succesfully provisioned the site, redirecting"]);
                                IHttpContextAccessor accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                                accessor.HttpContext.Response.Redirect("/");
                            }
                            else
                            {
                                var stringBuilder = new StringBuilder();
                                foreach (var error in setupContext.Errors)
                                {
                                    stringBuilder.Append($"{error.Key} : '{error.Value}'");
                                }
                                _logger.LogError(T["AutoSetup failed with errors: {0}", stringBuilder.ToString()]);
                            }
                        }
                    }
                }
            }

            return next;
        }

        private SetupContext GetSetupContext(AutoSetupOptions options, ISetupService setupService, ShellSettings shellSettings)
        {
            var recipes = setupService.GetSetupRecipesAsync()
                .GetAwaiter()
                .GetResult();

            var recipe = recipes.SingleOrDefault(r => r.Name == options.RecipeName);

            var setupContext = new SetupContext()
            {
                AdminEmail = options.AdminEmail,
                AdminPassword = options.AdminPassword,
                AdminUsername = options.AdminUsername,
                DatabaseConnectionString = options.DatabaseConnectionString,
                DatabaseProvider = options.DatabaseProvider,
                DatabaseTablePrefix = options.DatabaseTablePrefix,
                SiteName = options.SiteName,
                Recipe = recipe,
                SiteTimeZone = options.SiteTimeZone,
                Errors = new Dictionary<string, string>(),
                ShellSettings = shellSettings
            };

            return setupContext;
        }

        private void CreateDatabase(IServiceScope scope, AutoSetupOptions options, SetupContext setupContext)
        {
            // maybe this should be moved to yessql?

            switch (options.DatabaseProvider)
            {
                case "Postgres":
                    var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder(options.DatabaseConnectionString);
                    connectionStringBuilder.Database = connectionStringBuilder["EntityAdminDatabase"].ToString();
                    var connection = new Npgsql.NpgsqlConnection(connectionStringBuilder.ConnectionString);
                    connection.Open();
                    try
                    {
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = $"CREATE DATABASE {options.DatabaseName};";
                        cmd.ExecuteNonQuery();
                    }
                    catch (DbException)
                    {
                        if(connection.State == ConnectionState.Open)
                            connection.Close();
                    }
                    break;
                case "Sqlite":
                    // no need to create a database
                    break;
                default:
                    setupContext.Errors.Add("CreateDatabaseError", $"Unable to create database. Provider; {options.DatabaseProvider} is not supported");
                    break;
            }
        }
    }
}
