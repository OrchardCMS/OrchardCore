using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Setup.Options;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup
{
    public class AutoSetupStartupFilter : IStartupFilter
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer T;

        public AutoSetupStartupFilter(IShellHost shellHost, IStringLocalizer<AutoSetupStartupFilter> stringLocalizer, ILogger<AutoSetupStartupFilter> logger)
        {
            T = stringLocalizer;
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            var scopedServices = ShellScope.Services;
            var settings = ShellScope.Context.Settings;
            var state = ShellScope.Context.Settings.State;

            var siteIsUninitialized = state == OrchardCore.Environment.Shell.Models.TenantState.Uninitialized;

            if (siteIsUninitialized)
            {
                var optionsAccessor = scopedServices.GetRequiredService<IOptions<AutoSetupOptions>>();
                var options = optionsAccessor.Value;

                if (options != null)
                {
                    _logger.LogInformation("AutoSetup is initializing the site");
                    var validationContext = new ValidationContext(options, scopedServices, null);
                    var validationErrors = options.Validate(validationContext);

                    if (validationErrors.Any())
                    {
                        var stringBuilder = new StringBuilder();
                        foreach (var error in validationErrors)
                        {
                            stringBuilder.Append(error.ErrorMessage);
                        }
                        _logger.LogError("AutoSetup did not start, configuration has following errors: {errors}", stringBuilder.ToString());
                    }
                    else
                    {
                        var setupService = scopedServices.GetRequiredService<ISetupService>();

                        var setupContext = GetSetupContext(options, setupService, settings);

                        setupService.SetupAsync(setupContext)
                            .GetAwaiter()
                            .GetResult();

                        if (setupContext.Errors.Count == 0)
                        {
                            _logger.LogInformation("AutoSetup succesfully provisioned the site, redirecting");
                            IHttpContextAccessor accessor = scopedServices.GetRequiredService<IHttpContextAccessor>();
                            accessor.HttpContext.Response.Redirect("/");

                            // Complete the request
                            accessor.HttpContext.Response.CompleteAsync()
                                .GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            var stringBuilder = new StringBuilder();
                            foreach (var error in setupContext.Errors)
                            {
                                stringBuilder.Append($"{error.Key} : '{error.Value}'");
                            }
                            _logger.LogError("AutoSetup failed with errors: {errors}", stringBuilder.ToString());
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
    }
}
