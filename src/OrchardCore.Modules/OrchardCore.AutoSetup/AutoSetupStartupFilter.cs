using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.AutoSetup
{
    /// <summary>
    /// The filter which registers <see cref="AutoSetupMiddleware"/> to setup the site.
    /// </summary>
    public class AutoSetupStartupFilter : IStartupFilter
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSetupStartupFilter"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public AutoSetupStartupFilter(ILogger<AutoSetupStartupFilter> logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// The configure a middleware pipeline
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <returns>
        /// The <see cref="Action"/>.
        /// </returns>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            var scopedServices = ShellScope.Services;
            var state = ShellScope.Context.Settings.State;

            var siteIsUninitialized = state == OrchardCore.Environment.Shell.Models.TenantState.Uninitialized;

            if (siteIsUninitialized)
            {
                var optionsAccessor = scopedServices.GetRequiredService<IOptions<AutoSetupOptions>>();
                var options = optionsAccessor.Value;

                if (options != null)
                {
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
                        return builder =>
                        {
                            if (string.IsNullOrEmpty(options.TriggerSetupUrl))
                            {
                                builder.UseMiddleware<AutoSetupMiddleware>();
                            }
                            else
                            {
                                builder.Map(
                                    options.TriggerSetupUrl,
                                    applicationBuilder => applicationBuilder.UseMiddleware<AutoSetupMiddleware>());
                            }

                            next(builder);
                        };
                    }
                }
            }

            return next;
        }
    }
}
