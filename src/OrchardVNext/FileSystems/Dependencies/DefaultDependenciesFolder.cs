using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OrchardVNext.FileSystems.AppData;
using OrchardVNext.Utility.Extensions;

namespace OrchardVNext.FileSystems.Dependencies {
    public class DefaultDependenciesFolder : IDependenciesFolder {
        private const string BasePath = "Dependencies";
        private const string FileName = "dependencies.xml";
        private readonly IAppDataFolder _appDataFolder;
        private readonly InvalidationToken _writeThroughToken;

        public DefaultDependenciesFolder(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            _writeThroughToken = new InvalidationToken();
        }

        public bool DisableMonitoring { get; set; }

        private string PersistencePath {
            get { return _appDataFolder.Combine(BasePath, FileName); }
        }

        public DependencyDescriptor GetDescriptor(string moduleName) {
            return LoadDescriptors().SingleOrDefault(d => StringComparer.OrdinalIgnoreCase.Equals(d.Name, moduleName));
        }

        public IEnumerable<DependencyDescriptor> LoadDescriptors() {
            _appDataFolder.CreateDirectory(BasePath);

            _writeThroughToken.IsCurrent = true;

            return ReadDependencies(PersistencePath).ToReadOnlyCollection();
        }

        public void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors) {
            var existingDescriptors = LoadDescriptors().OrderBy(d => d.Name);
            var newDescriptors = dependencyDescriptors.OrderBy(d => d.Name);

            if (!newDescriptors.SequenceEqual(existingDescriptors, new DependencyDescriptorComparer())) {
                WriteDependencies(PersistencePath, dependencyDescriptors);
            }
        }

        private IEnumerable<DependencyDescriptor> ReadDependencies(string persistancePath) {
            Func<string, XName> ns = (name => XName.Get(name));
            Func<XElement, string, string> elem = (e, name) => e.Element(ns(name)).Value;

            if (!_appDataFolder.FileExists(persistancePath))
                return Enumerable.Empty<DependencyDescriptor>();

            using (var stream = _appDataFolder.OpenFile(persistancePath)) {
                XDocument document = XDocument.Load(stream);
                return document
                    .Elements(ns("Dependencies"))
                    .Elements(ns("Dependency"))
                    .Select(e => new DependencyDescriptor {
                        Name = elem(e, "ModuleName"),
                        VirtualPath = elem(e, "VirtualPath"),
                        LoaderName = elem(e, "LoaderName"),
                        References = e.Elements(ns("References")).Elements(ns("Reference")).Select(r => new DependencyReferenceDescriptor {
                            Name = elem(r, "Name"),
                            LoaderName = elem(r, "LoaderName"),
                            VirtualPath = elem(r, "VirtualPath")
                    })}).ToList();
            }
        }

        private void WriteDependencies(string persistancePath, IEnumerable<DependencyDescriptor> dependencies) {
            Func<string, XName> ns = (name => XName.Get(name));

            var document = new XDocument();
            document.Add(new XElement(ns("Dependencies")));
            var elements = dependencies.Select(d => new XElement("Dependency",
                                                                 new XElement(ns("ModuleName"), d.Name),
                                                                 new XElement(ns("VirtualPath"), d.VirtualPath),
                                                                 new XElement(ns("LoaderName"), d.LoaderName),
                                                                 new XElement(ns("References"), d.References
                                                                     .Select(r => new XElement(ns("Reference"),
                                                                        new XElement(ns("Name"), r.Name),
                                                                        new XElement(ns("LoaderName"), r.LoaderName),
                                                                        new XElement(ns("VirtualPath"), r.VirtualPath))).ToArray())));

            document.Root.Add(elements);

            using (var stream = _appDataFolder.CreateFile(persistancePath)) {
                document.Save(stream, SaveOptions.None);
            }

            // Ensure cache is invalidated right away, not waiting for file change notification to happen
            _writeThroughToken.IsCurrent = false;
        }

        private class InvalidationToken : ISingletonDependency {
            public bool IsCurrent { get; set; }
        }

        private class DependencyDescriptorComparer : EqualityComparer<DependencyDescriptor> {
            private readonly ReferenceDescriptorComparer _referenceDescriptorComparer = new ReferenceDescriptorComparer();

            public override bool Equals(DependencyDescriptor x, DependencyDescriptor y) {
                return
                    StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.LoaderName, y.LoaderName) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.VirtualPath, y.VirtualPath) &&
                    x.References.SequenceEqual(y.References, _referenceDescriptorComparer);
            }

            public override int GetHashCode(DependencyDescriptor obj) {
                return
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.LoaderName) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.VirtualPath) ^
                    obj.References.Aggregate(0, (a, entry) => a + _referenceDescriptorComparer.GetHashCode(entry));
            }
        }

        private class ReferenceDescriptorComparer : EqualityComparer<DependencyReferenceDescriptor> {
            public override bool Equals(DependencyReferenceDescriptor x, DependencyReferenceDescriptor y) {
                return
                    StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.LoaderName, y.LoaderName) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.VirtualPath, y.VirtualPath);

            }

            public override int GetHashCode(DependencyReferenceDescriptor obj) {
                return
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.LoaderName) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.VirtualPath);
            }
        }
    }
}