using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orchard.Localization
{
    public class PoFilesTranslationsProvider : ITranslationProvider
    {
        private readonly IExtensionManager _extensionsManager;
        private readonly PoParser _parser;
        private readonly string _root;
        private readonly string _rootContainer;
        private readonly string _shellContainer;
        private readonly string _shellName;

        public PoFilesTranslationsProvider(
            IExtensionManager extensionsManager,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings)
        {
            _extensionsManager = extensionsManager;
            _parser = new PoParser();

            _root = hostingEnvironment.ContentRootPath;
            _rootContainer = shellOptions.Value.ShellsRootContainerName;
            _shellContainer = shellOptions.Value.ShellsContainerName;
            _shellName = shellSettings.Name;
        }

        public void LoadTranslationsToDictionary(string cultureName, CultureDictionary dictionary)
        {
            var corePath = Path.Combine(_root, "Core", "App_Data", "Localization", cultureName, "orchard.core.po");
            dictionary.MergeTranslations(Load(corePath));

            foreach (var extension in _extensionsManager.GetExtensions())
            {
                string filename = null;
                switch (extension.Manifest.Type.ToLowerInvariant())
                {
                    case "module": filename = "orchard.module.po"; break;
                    case "theme": filename = "orchard.theme.po"; break;
                    default: continue;
                }

                var extensionPath = Path.Combine(_root, extension.SubPath, "App_Data", "Localization", cultureName, filename);
                dictionary.MergeTranslations(Load(extensionPath));
            }

            var rootLocalizationPath = Path.Combine(_root, _rootContainer, "Localization", cultureName, "orchard.root.po");
            dictionary.MergeTranslations(Load(rootLocalizationPath));

            var shellPath = Path.Combine(_root, _rootContainer, _shellContainer, _shellName, "Localization", cultureName, "orchard.po");
            dictionary.MergeTranslations(Load(shellPath));
        }

        private IEnumerable<CultureDictionaryRecord> Load(string path)
        {
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return _parser.Parse(reader);
                    }
                }
            }

            return new CultureDictionaryRecord[0];
        }
    }
}
