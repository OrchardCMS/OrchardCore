using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Recipes.Models;
using Orchard.Environment.Shell.Models;

namespace Orchard.Recipes.Providers.Executors
{
    public class ActivateShellStep : RecipeExecutionStep
    {
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;

        public ActivateShellStep(
            ShellSettings shellSettings, 
            IShellSettingsManager shellSettingsManager, 
            ILoggerFactory logger,
            IStringLocalizer<ActivateShellStep> localizer)
            : base(logger, localizer)
        {
            _shellSettings = shellSettings;
            _shellSettingsManager = shellSettingsManager;
        }

        public override string Name { get { return "ActivateShell"; } }

        public override void Execute(RecipeExecutionContext context)
        {
            _shellSettings.State = TenantState.Running;
            _shellSettingsManager.SaveSettings(_shellSettings);
        }
    }
}
