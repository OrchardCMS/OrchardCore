using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Setup.Services;

namespace OrchardCore.AutoSetup
{
    /// <summary>
    /// The auto setup middleware.
    /// </summary>
    public class AutoSetupMiddleware
    {
        /// <summary>
        /// The _next.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The auto-setup options.
        /// </summary>
        private readonly IOptions<AutoSetupOptions> _setupOptions;

        /// <summary>
        /// The _logger.
        /// </summary>
        private readonly ILogger<AutoSetupMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSetupMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="setupOptions">
        /// The auto-setup Options.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public AutoSetupMiddleware(RequestDelegate next, IOptions<AutoSetupOptions> setupOptions, ILogger<AutoSetupMiddleware> logger)
        {
            _logger = logger;
            _next = next;
            _setupOptions = setupOptions;
        }

        /// <summary>
        /// The middleware auto-setup invoke.
        /// </summary>
        /// <param name="httpContext">
        /// The http context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var settings = ShellScope.Context.Settings;

            _logger.LogInformation("AutoSetup is initializing the site");

            var setupService = httpContext.RequestServices.GetRequiredService<ISetupService>();

            var setupContext = GetSetupContext(_setupOptions.Value, setupService, settings);

            await setupService.SetupAsync(setupContext);

            if (setupContext.Errors.Count == 0)
            {
                _logger.LogInformation("AutoSetup successfully provisioned the site, redirecting");

                httpContext.Response.Redirect("/");

                // Complete the request
                // await httpContext.Response.CompleteAsync();
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

            await _next.Invoke(httpContext);
        }

        /// <summary>
        /// Get setup context from the configuration.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="setupService">
        /// The setup service.
        /// </param>
        /// <param name="shellSettings">
        /// The shell settings.
        /// </param>
        /// <returns>
        /// The <see cref="SetupContext"/>.
        /// </returns>
        private static SetupContext GetSetupContext(AutoSetupOptions options, ISetupService setupService, ShellSettings shellSettings)
        {
            var recipes = setupService.GetSetupRecipesAsync()
                .GetAwaiter()
                .GetResult();

            var recipe = recipes.SingleOrDefault(r => r.Name == options.RecipeName);

            var setupContext = new SetupContext
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
