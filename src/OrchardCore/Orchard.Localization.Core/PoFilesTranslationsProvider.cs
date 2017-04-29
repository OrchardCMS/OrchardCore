using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using Orchard.Localization.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orchard.Localization.Core
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
            LoadFileToDictionary(corePath, dictionary);

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
                LoadFileToDictionary(extensionPath, dictionary);
            }

            var rootLocalizationPath = Path.Combine(_root, _rootContainer, "Localization", cultureName, "orchard.root.po");
            LoadFileToDictionary(rootLocalizationPath, dictionary);

            var shellPath = Path.Combine(_root, _rootContainer, _shellContainer, _shellName, "Localization", cultureName, "orchard.po");
            LoadFileToDictionary(shellPath, dictionary);
        }

        private void LoadFileToDictionary(string path, CultureDictionary dictionary)
        {
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        dictionary.MergeTranslations(_parser.Parse(reader));
                    }
                }
            }
        }
    }
}
