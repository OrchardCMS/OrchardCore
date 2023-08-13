using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Themes.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly INotifier _notifier;
        private readonly ISiteThemeService _siteThemeService;
        protected readonly IHtmlLocalizer H;

        public ThemeService(
            IExtensionManager extensionManager,
            IShellFeaturesManager shellFeaturesManager,
            ISiteThemeService siteThemeService,
            IHtmlLocalizer<ThemeService> htmlLocalizer,
            INotifier notifier)
        {
            _extensionManager = extensionManager;
            _shellFeaturesManager = shellFeaturesManager;
            _siteThemeService = siteThemeService;

            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task DisableThemeFeaturesAsync(string themeName)
        {
            var themes = new Queue<string>();
            while (themeName != null)
            {
                if (themes.Contains(themeName))
                    throw new InvalidOperationException(H["The theme \"{0}\" is already in the stack of themes that need features disabled.", themeName].ToString());
                var theme = _extensionManager.GetExtension(themeName);
                if (theme == null)
                    break;
                themes.Enqueue(themeName);

                themeName = !String.IsNullOrWhiteSpace(theme.Manifest.Name)
                    ? theme.Manifest.Name
                    : null;
            }

            var currentTheme = await _siteThemeService.GetSiteThemeNameAsync();

            while (themes.Count > 0)
            {
                var themeId = themes.Dequeue();

                // Not disabling base theme if it's the current theme.
                if (themeId != currentTheme)
                {
                    await DisableFeaturesAsync(new[] { themeId }, true);
                }
            }
        }

        public async Task EnableThemeFeaturesAsync(string themeName)
        {
            var themes = new Stack<string>();
            while (themeName != null)
            {
                if (themes.Contains(themeName))
                    throw new InvalidOperationException(H["The theme \"{0}\" is already in the stack of themes that need features enabled.", themeName].ToString());
                themes.Push(themeName);

                // TODO: MWP: probably follow on issue: should this be recursive? maybe with a depth limit? i.e. base3->base2->base1 ...
                var extensionInfo = _extensionManager.GetExtension(themeName);
                var theme = new ThemeExtensionInfo(extensionInfo);
                // Along similar lines as with ModuleAttribute, internal safety checks handled within the attribute itself
                themeName = theme.BaseTheme;
            }

            while (themes.Count > 0)
            {
                var themeId = themes.Pop();

                await EnableFeaturesAsync(new[] { themeId }, true);
            }
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        public Task EnableFeaturesAsync(IEnumerable<string> featureIds)
        {
            return EnableFeaturesAsync(featureIds, false);
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        public async Task EnableFeaturesAsync(IEnumerable<string> featureIds, bool force)
        {
            var featuresToEnable = _extensionManager
                .GetFeatures()
                .Where(x => featureIds.Contains(x.Id));

            var enabledFeatures = await _shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force);
            foreach (var enabledFeature in enabledFeatures)
            {
                await _notifier.SuccessAsync(H["{0} was enabled.", enabledFeature.Name]);
            }
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        public Task DisableFeaturesAsync(IEnumerable<string> featureIds)
        {
            return DisableFeaturesAsync(featureIds, false);
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        public async Task DisableFeaturesAsync(IEnumerable<string> featureIds, bool force)
        {
            var featuresToDisable = _extensionManager
                .GetFeatures()
                .Where(x => featureIds.Contains(x.Id));

            var features = await _shellFeaturesManager.DisableFeaturesAsync(featuresToDisable, force);
            foreach (var feature in features)
            {
                await _notifier.SuccessAsync(H["{0} was disabled.", feature.Name]);
            }
        }
    }
}
