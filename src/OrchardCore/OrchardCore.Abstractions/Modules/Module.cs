using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Embedded;

namespace OrchardCore.Modules
{
    using Manifest;

    public class Module
    {
        public const string WebRootPath = "wwwroot";
        public const string WebRoot = WebRootPath + "/";

        private readonly string _baseNamespace;
        private readonly DateTimeOffset _lastModified;
        private readonly Dictionary<string, IFileInfo> _fileInfos = new();

        // TODO: MWP: trace back to usage, etc...
        // TODO: MWP: perhaps we filter up front so we are not discovering all this during ctor...
        // TODO: MWP: 'that' being the Module Context, etc...
        /// <summary>
        /// Constructs an instance of the Module.
        /// </summary>
        /// <param name="assemblyName">The assembly name to use when loading itself.</param>
        /// <param name="isApplication">Whether the Module may be considered to be the &quot;Application&quot;.</param>
        public Module(string assemblyName, bool isApplication = false)
        {
            if (!String.IsNullOrWhiteSpace(assemblyName))
            {
                Assembly = Assembly.Load(new AssemblyName(assemblyName));

                Assets = Assembly.GetCustomAttributes<ModuleAssetAttribute>()
                    .Select(a => new Asset(a.Asset)).ToArray();

                AssetPaths = Assets.Select(a => a.ModuleAssetPath).ToArray();

                // TODO: MWP: so we are 'aware' multiple instances are popping up, then...
                var moduleInfos = Assembly.GetCustomAttributes<ModuleAttribute>();

                ModuleInfo =
                    moduleInfos.Where(f => f is not ModuleMarkerAttribute).FirstOrDefault()
                    ?? moduleInfos.Where(f => f is ModuleMarkerAttribute).FirstOrDefault()
                    // This is better use the default parameterless ctor and assign the property.
                    ?? new ModuleAttribute() { Name = assemblyName };

                var features = Assembly.GetCustomAttributes<Manifest.FeatureAttribute>()
                    .Where(f => f is not ModuleAttribute).ToList();

                if (isApplication)
                {
                    ModuleInfo.Name = Application.ModuleName;
                    ModuleInfo.Description = Application.ModuleDescription;
                    ModuleInfo.Priority = Application.ModulePriority;
                    ModuleInfo.Category = Application.ModuleCategory;
                    ModuleInfo.DefaultTenantOnly = true;

                    // Adds the application primary feature.
                    features.Insert(0, new Manifest.FeatureAttribute(
                        assemblyName,
                        Application.ModuleName,
                        Application.ModuleCategory,
                        Application.ModulePriority,
                        Application.ModuleDescription,
                        null,
                        true,
                        default,
                        default
                    ));

                    // Adds the application default feature.
                    features.Insert(1, new Manifest.FeatureAttribute(
                        Application.DefaultFeatureId,
                        Application.DefaultFeatureName,
                        Application.ModuleCategory,
                        Application.ModulePriority,
                        Application.DefaultFeatureDescription,
                        null,
                        true,
                        default,
                        default
                    ));
                }

                ModuleInfo.Features.AddRange(features);

                // The 'ModuleInfo.Id' allows a module project to change its 'AssemblyName'
                // without to update the code. If not provided, the assembly name is used.

                var logicalName = ModuleInfo.Id ?? assemblyName;

                Name = ModuleInfo.Id = logicalName;
                SubPath = Application.ModulesRoot + Name;
                Root = SubPath + '/';
            }
            else
            {
                Name = Root = SubPath = String.Empty;
                Assets = Enumerable.Empty<Asset>();
                AssetPaths = Enumerable.Empty<string>();
                ModuleInfo = new ModuleAttribute();
            }

            _baseNamespace = Name + '.';
            _lastModified = DateTimeOffset.UtcNow;

            if (!String.IsNullOrEmpty(Assembly?.Location))
            {
                try
                {
                    _lastModified = File.GetLastWriteTimeUtc(Assembly.Location);
                }
                catch (PathTooLongException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        public string Name { get; }
        public string Root { get; }
        public string SubPath { get; }
        public Assembly Assembly { get; }
        public IEnumerable<Asset> Assets { get; }
        public IEnumerable<string> AssetPaths { get; }
        public ModuleAttribute ModuleInfo { get; }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (!_fileInfos.TryGetValue(subpath, out var fileInfo))
            {
                if (!AssetPaths.Contains(Root + subpath))
                {
                    return new NotFoundFileInfo(subpath);
                }

                lock (_fileInfos)
                {
                    if (!_fileInfos.TryGetValue(subpath, out fileInfo))
                    {
                        var resourcePath = _baseNamespace + subpath.Replace('/', '>');
                        var fileName = Path.GetFileName(subpath);

                        if (Assembly.GetManifestResourceInfo(resourcePath) == null)
                        {
                            return new NotFoundFileInfo(fileName);
                        }

                        _fileInfos[subpath] = fileInfo = new EmbeddedResourceFileInfo(
                            Assembly, resourcePath, fileName, _lastModified);
                    }
                }
            }

            return fileInfo;
        }
    }
}
