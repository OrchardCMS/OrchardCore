using System.Collections.Generic;
using System.Linq;
using OrchardVNext;

namespace OrchardVNext.FileSystems.Dependencies {
    public class DependencyDescriptor {
        public DependencyDescriptor() {
            References = Enumerable.Empty<DependencyReferenceDescriptor>();
        }
        public string Name { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
        public IEnumerable<DependencyReferenceDescriptor> References { get; set; }
    }

    public class DependencyReferenceDescriptor {
        public string Name { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
    }

    public interface IDependenciesFolder : ISingletonDependency {
        DependencyDescriptor GetDescriptor(string moduleName);
        IEnumerable<DependencyDescriptor> LoadDescriptors();
        void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors);
    }
}
