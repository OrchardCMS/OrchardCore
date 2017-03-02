using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Extensions;
using Orchard.DisplayManagement.Notify;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;

namespace Orchard.Themes.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly ISiteThemeService _siteThemeService;

        public ThemeService(
            IExtensionManager extensionManager,
            IShellFeaturesManager shellFeaturesManager,
            ISiteThemeService siteThemeService,
            ILogger<ThemeService> logger,
            IHtmlLocalizer<AdminMenu> htmlLocalizer,
            INotifier notifier)
        {
            _extensionManager = extensionManager;
            _shellFeaturesManager = shellFeaturesManager;
            _siteThemeService = siteThemeService;
            
            _logger = logger;
            _notifier = notifier;
            T = htmlLocalizer;
        }

        public IHtmlLocalizer T { get; set; }

        public async Task DisableThemeFeaturesAsync(string themeName)
        {
            var themes = new Queue<string>();
            while (themeName != null)
            {
                if (themes.Contains(themeName))
                    throw new InvalidOperationException(T["The theme \"{0}\" is already in the stack of themes that need features disabled.", themeName].ToString());
                var theme = _extensionManager.GetExtension(themeName);
                if (theme == null)
                    break;
                themes.Enqueue(themeName);

                themeName = !string.IsNullOrWhiteSpace(theme.Manifest.Name)
                    ? theme.Manifest.Name
                    : null;
            }

            var currentTheme = await _siteThemeService.GetCurrentThemeNameAsync();

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
                    throw new InvalidOperationException(T["The theme \"{0}\" is already in the stack of themes that need features enabled.", themeName].ToString());
                themes.Push(themeName);

                var extensionInfo = _extensionManager.GetExtension(themeName);
                var theme = new ThemeExtensionInfo(extensionInfo);
                themeName = !string.IsNullOrWhiteSpace(theme.BaseTheme)
                    ? theme.BaseTheme
                    : null;
            }

            while (themes.Count > 0)
            {
                var themeId = themes.Pop();

                await EnableFeaturesAsync(new[] { themeId }, true);

                //foreach (var featureId in enabledFeatures)
                //{
                //    if (themeId != featureId)
                //    {
                //        var availableFeatures = _extensionManager.GetFeatures();
                //        var featureName = availableFeatures.First(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                //        _notifier.Success(T["{0} was enabled", featureName]);
                //    }
                //}
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
                _notifier.Success(T["{0} was enabled", enabledFeature.Name]);
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
                _notifier.Success(T["{0} was disabled", feature.Name]);
            }
        }

        //public ExtensionDescriptor GetRequestTheme(RequestContext requestContext)
        //{
        //    var requestTheme = _themeSelectors
        //        .SelecT[x => x.GetTheme(requestContext))
        //        .Where(x => x != null)
        //        .OrderByDescending(x => x.Priority);

        //    if (requestTheme.Count() < 1)
        //        return null;

        //    foreach (var theme in requestTheme)
        //    {
        //        var t = _extensionManager.GetExtension(theme.ThemeName);
        //        if (t != null)
        //            return t;
        //    }

        //    return _extensionManager.GetExtension("SafeMode");
        //}

        ///// <summary>
        ///// Loads only installed themes
        ///// </summary>
        //public IEnumerable<ExtensionDescriptor> GetInstalledThemes()
        //{
        //    return GetThemes(_extensionManager.AvailableExtensions());
        //}

        //private IEnumerable<ExtensionDescriptor> GetThemes(IEnumerable<ExtensionDescriptor> extensions)
        //{
        //    var themes = new List<ExtensionDescriptor>();
        //    foreach (var descriptor in extensions)
        //    {

        //        if (!DefaultExtensionTypes.IsTheme(descriptor.ExtensionType))
        //        {
        //            continue;
        //        }

        //        ExtensionDescriptor theme = descriptor;

        //        if (theme.Tags == null || !theme.Tags.Contains("hidden"))
        //        {
        //            themes.Add(theme);
        //        }
        //    }
        //    return themes;
        //}

        ///// <summary>
        ///// Determines if a theme was recently installed by using the project's last written time.
        ///// </summary>
        ///// <param name="extensionDescriptor">The extension descriptor.</param>
        //public bool IsRecentlyInstalled(ExtensionDescriptor extensionDescriptor)
        //{
        //    DateTime lastWrittenUtc = _cacheManager.Get(extensionDescriptor, descriptor =>
        //    {
        //        string projectFile = GetManifestPath(extensionDescriptor);
        //        if (!string.IsNullOrEmpty(projectFile))
        //        {
        //            // If project file was modified less than 24 hours ago, the module was recently deployed
        //            return _virtualPathProvider.GetFileLastWriteTimeUtc(projectFile);
        //        }

        //        return DateTime.UtcNow;
        //    });

        //    return DateTime.UtcNow.Subtract(lastWrittenUtc) < new TimeSpan(1, 0, 0, 0);
        //}

        //private string GetManifestPath(ExtensionDescriptor descriptor)
        //{
        //    string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Id,
        //                                               "theme.txt");

        //    if (!_virtualPathProvider.FileExists(projectPath))
        //    {
        //        return null;
        //    }

        //    return projectPath;
        //}

        //private void GenerateWarning(string messageFormat, string featureName, IEnumerable<string> featuresInQuestion)
        //{
        //    if (featuresInQuestion.Count) < 1)
        //        return;

        //    Services.Notifier.Warning(T(
        //        messageFormat,
        //        featureName,
        //        featuresInQuestion.Count() > 1
        //            ? string.Join("",
        //                          featuresInQuestion.Select(
        //                              (fn, i) =>
        //                              T(i == featuresInQuestion.Count() - 1
        //                                    ? "{0}"
        //                                    : (i == featuresInQuestion.Count() - 2
        //                                           ? "{0} and "
        //                                           : "{0}, "), fn).ToString()).ToArray())
        //            : featuresInQuestion.First()));
        //}

        //public void DisablePreviewFeatures(IEnumerable<string> features)
        //{
        //    foreach (var featureId in _featureManager.DisableFeatures(features, true))
        //    {
        //        var featureName = _featureManager.GetAvailableFeatures().First(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
        //        Services.Notifier.Success(T("{0} was disabled", featureName));
        //    }
        //}
    }
}
