using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.AspNetCore.Modules
{
    public static class DefaultAssemblyDiscoveryProvider
    {
        public static IEnumerable<Assembly> DiscoverAssemblies(IList<Assembly> entryPointAssemblies, ISet<string> referenceAssemblies)
        {
            var discoveredAssemblies = new List<Assembly>();

            foreach (var entryPointAssembly in entryPointAssemblies)
            {
                var context = DependencyContext.Load(entryPointAssembly);

                var candidateAssemblies = GetCandidateAssemblies(entryPointAssembly, context, referenceAssemblies);

                discoveredAssemblies.AddRange(candidateAssemblies);
            }

            // Do I need a distinct?
            return discoveredAssemblies.OrderBy(a => a.FullName).Distinct();
        }

        internal static IEnumerable<Assembly> GetCandidateAssemblies(
            Assembly entryAssembly, 
            DependencyContext dependencyContext,
            ISet<string> referenceAssemblies)
        {
            if (dependencyContext == null)
            {
                // Use the entry assembly as the sole candidate.
                return new[] { entryAssembly };
            }

            return GetCandidateLibraries(dependencyContext, referenceAssemblies)
                .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);
        }

        // Returns a list of libraries that references the assemblies in <see cref="ReferenceAssemblies"/>.
        // By default it returns all assemblies that reference any of the primary MVC assemblies
        // while ignoring MVC assemblies.
        // Internal for unit testing
        internal static IEnumerable<RuntimeLibrary> GetCandidateLibraries(DependencyContext dependencyContext, ISet<string> referenceAssemblies)
        {
            if (referenceAssemblies == null)
            {
                return Enumerable.Empty<RuntimeLibrary>();
            }

            var candidatesResolver = new CandidateResolver(dependencyContext.RuntimeLibraries, referenceAssemblies);
            return candidatesResolver.GetCandidates();
        }

        private class CandidateResolver
        {
            private readonly IDictionary<string, Dependency> _dependencies;

            public CandidateResolver(IReadOnlyList<RuntimeLibrary> dependencies, ISet<string> referenceAssemblies)
            {
                var dependenciesWithNoDuplicates = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);
                foreach (var dependency in dependencies)
                {
                    if (dependenciesWithNoDuplicates.ContainsKey(dependency.Name))
                    {
                        throw new InvalidOperationException("Resources.FormatCandidateResolver_DifferentCasedReference(dependency.Name)");
                    }
                    dependenciesWithNoDuplicates.Add(dependency.Name, CreateDependency(dependency, referenceAssemblies));
                }

                _dependencies = dependenciesWithNoDuplicates;
            }

            private Dependency CreateDependency(RuntimeLibrary library, ISet<string> referenceAssemblies)
            {
                var classification = DependencyClassification.Unknown;
                if (referenceAssemblies.Contains(library.Name))
                {
                    classification = DependencyClassification.Reference;
                }

                return new Dependency(library, classification);
            }

            private DependencyClassification ComputeClassification(string dependency)
            {
                Debug.Assert(_dependencies.ContainsKey(dependency));

                var candidateEntry = _dependencies[dependency];
                if (candidateEntry.Classification != DependencyClassification.Unknown)
                {
                    return candidateEntry.Classification;
                }
                else
                {
                    var classification = DependencyClassification.NotCandidate;
                    foreach (var candidateDependency in candidateEntry.Library.Dependencies)
                    {
                        var dependencyClassification = ComputeClassification(candidateDependency.Name);
                        if (dependencyClassification == DependencyClassification.Candidate ||
                            dependencyClassification == DependencyClassification.Reference)
                        {
                            classification = DependencyClassification.Candidate;
                            break;
                        }
                    }

                    candidateEntry.Classification = classification;

                    return classification;
                }
            }

            public IEnumerable<RuntimeLibrary> GetCandidates()
            {
                foreach (var dependency in _dependencies)
                {
                    if (ComputeClassification(dependency.Key) == DependencyClassification.Candidate)
                    {
                        yield return dependency.Value.Library;
                    }
                }
            }

            private class Dependency
            {
                public Dependency(RuntimeLibrary library, DependencyClassification classification)
                {
                    Library = library;
                    Classification = classification;
                }

                public RuntimeLibrary Library { get; }

                public DependencyClassification Classification { get; set; }

                public override string ToString()
                {
                    return $"Library: {Library.Name}, Classification: {Classification}";
                }
            }

            private enum DependencyClassification
            {
                Unknown = 0,
                Candidate = 1,
                NotCandidate = 2,
                Reference = 3
            }
        }
    }
}