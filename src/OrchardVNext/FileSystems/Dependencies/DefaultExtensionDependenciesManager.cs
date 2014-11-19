using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OrchardVNext.FileSystems.AppData;

namespace OrchardVNext.FileSystems.Dependencies {
    /// <summary>
    /// Similar to "Dependencies.xml" file, except we also store "GetFileHash" result for every 
    /// VirtualPath entry. This is so that if any virtual path reference in the file changes,
    /// the file stored by this component will also change.
    /// </summary>
    public class DefaultExtensionDependenciesManager : IExtensionDependenciesManager {
        private const string BasePath = "Dependencies";
        private const string FileName = "dependencies.compiled.xml";
        private readonly IAppDataFolder _appDataFolder;
        private readonly InvalidationToken _writeThroughToken;

        public DefaultExtensionDependenciesManager(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            _writeThroughToken = new InvalidationToken();
        }

        public bool DisableMonitoring { get; set; }

        private string PersistencePath {
            get { return _appDataFolder.Combine(BasePath, FileName); }
        }

        public void StoreDependencies(IEnumerable<DependencyDescriptor> dependencyDescriptors, Func<DependencyDescriptor, string> fileHashProvider) {
            Logger.Information("Storing module dependency file.");

            var newDocument = CreateDocument(dependencyDescriptors, fileHashProvider);
            var previousDocument = ReadDocument(PersistencePath);
            if (XNode.DeepEquals(newDocument.Root, previousDocument.Root)) {
                Logger.Debug("Existing document is identical to new one. Skipping save.");
            }
            else {
                WriteDocument(PersistencePath, newDocument);
            }

            Logger.Information("Done storing module dependency file.");
        }

        public IEnumerable<string> GetVirtualPathDependencies(string extensionId) {
            var descriptor = GetDescriptor(extensionId);
            if (descriptor != null && IsSupportedLoader(descriptor.LoaderName)) {
                // Currently, we return the same file for every module. An improvement would be to return
                // a specific file per module (this would decrease the number of recompilations needed
                // when modules change on disk).
                yield return _appDataFolder.GetVirtualPath(PersistencePath);
            }
        }

        public ActivatedExtensionDescriptor GetDescriptor(string extensionId) {
            return LoadDescriptors().FirstOrDefault(d => d.ExtensionId.Equals(extensionId, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ActivatedExtensionDescriptor> LoadDescriptors() {
                _appDataFolder.CreateDirectory(BasePath);

                return ReadDescriptors(PersistencePath).ToList();
        }

        private XDocument CreateDocument(IEnumerable<DependencyDescriptor> dependencies, Func<DependencyDescriptor, string> fileHashProvider) {
            Func<string, XName> ns = (name => XName.Get(name));

            var elements = dependencies
                .Where(dep => IsSupportedLoader(dep.LoaderName))
                .OrderBy(dep => dep.Name, StringComparer.OrdinalIgnoreCase)
                .Select(descriptor =>
                        new XElement(ns("Dependency"),
                            new XElement(ns("ExtensionId"), descriptor.Name),
                            new XElement(ns("LoaderName"), descriptor.LoaderName),
                            new XElement(ns("VirtualPath"), descriptor.VirtualPath),
                            new XElement(ns("Hash"), fileHashProvider(descriptor))));

            return new XDocument(new XElement(ns("Dependencies"), elements.ToArray()));
        }

        private IEnumerable<ActivatedExtensionDescriptor> ReadDescriptors(string persistancePath) {
            Func<string, XName> ns = (name => XName.Get(name));
            Func<XElement, string, string> elem = (e, name) => e.Element(ns(name)).Value;

            XDocument document = ReadDocument(persistancePath);
            return document
                .Elements(ns("Dependencies"))
                .Elements(ns("Dependency"))
                .Select(e => new ActivatedExtensionDescriptor {
                    ExtensionId = elem(e, "ExtensionId"),
                    VirtualPath = elem(e, "VirtualPath"),
                    LoaderName = elem(e, "LoaderName"),
                    Hash = elem(e, "Hash"),
                }).ToList();
        }

        private bool IsSupportedLoader(string loaderName) {
            //Note: this is hard-coded for now, to avoid adding more responsibilities to the IExtensionLoader
            //      implementations.
            return
                loaderName == "DynamicExtensionLoader" ||
                loaderName == "PrecompiledExtensionLoader";
        }

        private void WriteDocument(string persistancePath, XDocument document) {
            _writeThroughToken.IsCurrent = false;
            using (var stream = _appDataFolder.CreateFile(persistancePath)) {
                document.Save(stream, SaveOptions.None);
            }
        }

        private XDocument ReadDocument(string persistancePath) {
            if (!_appDataFolder.FileExists(persistancePath))
                return new XDocument();

            try {
                using (var stream = _appDataFolder.OpenFile(persistancePath)) {
                    return XDocument.Load(stream);
                }
            }
            catch (Exception e) {
                Logger.Information(e, "Error reading file '{0}'. Assuming empty.", persistancePath);
                return new XDocument();
            }
        }

        private class InvalidationToken : ISingletonDependency {
            public bool IsCurrent { get; set; }
        }
    }
}