//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.Loader;
//using Microsoft.Extensions.DependencyModel;

//namespace Microsoft.AspNetCore.Modules
//{
//    public class ModularAssemblyDiscoveryProvider
//    {
//        public static IEnumerable<Assembly> DiscoverAssemblies(Assembly entryAssembly, ISet<string> referencedAssemblies)
//        {
//            var context = DependencyContext.Load(entryAssembly);

//            var candidateAssemblies = GetCandidateAssemblies(entryAssembly, context, referencedAssemblies);
//        }

//        internal static IEnumerable<Assembly> GetCandidateAssemblies(Assembly entryAssembly, DependencyContext dependencyContext, ISet<string> referencedAssemblies)
//        {
//            if (dependencyContext == null)
//            {
//                // Use the entry assembly as the sole candidate.
//                return new[] { entryAssembly };
//            }

//            return GetCandidateLibraries(dependencyContext, referencedAssemblies)
//                .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
//                .Select(Assembly.Load);
//        }

//        // Returns a list of libraries that references the assemblies in <see cref="ReferenceAssemblies"/>.
//        // By default it returns all assemblies that reference any of the primary MVC assemblies
//        // while ignoring MVC assemblies.
//        // Internal for unit testing
//        internal static IEnumerable<RuntimeLibrary> GetCandidateLibraries(DependencyContext dependencyContext, ISet<string> referencedAssemblies)
//        {
//            if (referencedAssemblies == null)
//            {
//                return Enumerable.Empty<RuntimeLibrary>();
//            }

//            var candidatesResolver = new CandidateResolver(dependencyContext.RuntimeLibraries, referencedAssemblies);
//            return candidatesResolver.GetCandidates();
//        }

//        private class CandidateResolver
//        {
//            private readonly IDictionary<string, Dependency> _dependencies;

//            public CandidateResolver(IReadOnlyList<RuntimeLibrary> dependencies, ISet<string> referenceAssemblies)
//            {
//                var dependenciesWithNoDuplicates = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);
//                foreach (var dependency in dependencies)
//                {
//                    if (dependenciesWithNoDuplicates.ContainsKey(dependency.Name))
//                    {
//                        throw new InvalidOperationException(
//                            $"Different cased reference ({dependency.Name})");
//                    }
//                    dependenciesWithNoDuplicates.Add(dependency.Name, CreateDependency(dependency, referenceAssemblies));
//                }

//                _dependencies = dependenciesWithNoDuplicates;
//            }

//            private Dependency CreateDependency(RuntimeLibrary library, ISet<string> referenceAssemblies)
//            {
//                var classification = DependencyClassification.Unknown;
//                if (referenceAssemblies.Contains(library.Name))
//                {
//                    classification = DependencyClassification.MvcReference;
//                }

//                return new Dependency(library, classification);
//            }

//            private DependencyClassification ComputeClassification(string dependency)
//            {
//                Debug.Assert(_dependencies.ContainsKey(dependency));

//                var candidateEntry = _dependencies[dependency];
//                if (candidateEntry.Classification != DependencyClassification.Unknown)
//                {
//                    return candidateEntry.Classification;
//                }
//                else
//                {
//                    var classification = DependencyClassification.NotCandidate;
//                    foreach (var candidateDependency in candidateEntry.Library.Dependencies)
//                    {
//                        var dependencyClassification = ComputeClassification(candidateDependency.Name);
//                        if (dependencyClassification == DependencyClassification.Candidate ||
//                            dependencyClassification == DependencyClassification.MvcReference)
//                        {
//                            classification = DependencyClassification.Candidate;
//                            break;
//                        }
//                    }

//                    candidateEntry.Classification = classification;

//                    return classification;
//                }
//            }

//            public IEnumerable<RuntimeLibrary> GetCandidates()
//            {
//                foreach (var dependency in _dependencies)
//                {
//                    if (ComputeClassification(dependency.Key) == DependencyClassification.Candidate)
//                    {
//                        yield return dependency.Value.Library;
//                    }
//                }
//            }

//            private class Dependency
//            {
//                public Dependency(RuntimeLibrary library, DependencyClassification classification)
//                {
//                    Library = library;
//                    Classification = classification;
//                }

//                public RuntimeLibrary Library { get; }

//                public DependencyClassification Classification { get; set; }

//                public override string ToString()
//                {
//                    return $"Library: {Library.Name}, Classification: {Classification}";
//                }
//            }

//            private enum DependencyClassification
//            {
//                Unknown = 0,
//                Candidate = 1,
//                NotCandidate = 2,
//                MvcReference = 3
//            }
//        }




//        public IEnumerable<Assembly> GetAssemblies(IList<Assembly> assemblies)
//        {
//            var loadedContextAssemblies = new List<Assembly>();
//            var assemblyNames = new HashSet<string>();

//            foreach (var assembly in assemblies)
//            {
//                var currentAssemblyName =
//                    Path.GetFileNameWithoutExtension(assembly.Location);

//                if (assemblyNames.Add(currentAssemblyName))
//                {
//                    loadedContextAssemblies.Add(assembly);
//                }
//                loadedContextAssemblies.AddRange(GetAssemblies(assemblyNames, assembly));
//            }
//            return loadedContextAssemblies;
//        }

//        private static IList<Assembly> GetAssemblies(HashSet<string> assemblyNames, Assembly assembly)
//        {
//            var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
//            var referencedAssemblyNames = assembly.GetReferencedAssemblies()
//                .Where(ass => !assemblyNames.Contains(ass.Name));

//            var locations = new List<Assembly>();

//            foreach (var referencedAssemblyName in referencedAssemblyNames)
//            {
//                if (assemblyNames.Add(referencedAssemblyName.Name))
//                {
//                    var referencedAssembly = loadContext
//                        .LoadFromAssemblyName(referencedAssemblyName);

//                    locations.Add(referencedAssembly);

//                    locations.AddRange(GetAssemblies(assemblyNames, referencedAssembly));
//                }
//            }

//            return locations;
//        }

//        //public IEnumerable<TypeInfo> GetModularTypes()
//        //{
//        //    return _shellBlueprint.Dependencies.Select(x => x.Type.GetTypeInfo());
//        //}
//    }
//}
