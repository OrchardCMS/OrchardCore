using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Modules
{
    public static class DefaultModularAssemblyDiscoveryProvider
    {
        // Returns a list of assemblies that references the assemblies in referenceAssemblies
        public static IEnumerable<Assembly> GetCandidateAssemblies(IEnumerable<Assembly> assemblies, ISet<string> referenceAssemblies)
        {
            if (!assemblies.Any() || !referenceAssemblies.Any())
            {
                return Enumerable.Empty<Assembly>();
            }

            var candidatesResolver = new CandidateResolver(assemblies, referenceAssemblies);
            return candidatesResolver.GetCandidates();
        }

        private class CandidateResolver
        {
            private readonly IDictionary<string, Dependency> _dependencies = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);

            public CandidateResolver(IEnumerable<Assembly> assemblies, ISet<string> referenceAssemblies)
            {
                var dependenciesWithNoDuplicates = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);
                foreach (var assembly in assemblies)
                {
                    if (!dependenciesWithNoDuplicates.ContainsKey(assembly.GetName().Name))
                    {
                        dependenciesWithNoDuplicates.Add(assembly.GetName().Name, CreateDependency(assembly, referenceAssemblies));
                    }
                }

                _dependencies = dependenciesWithNoDuplicates;
            }

            private Dependency CreateDependency(Assembly assembly, ISet<string> referenceAssemblies)
            {
                var classification = DependencyClassification.Unknown;
                if (referenceAssemblies.Contains(assembly.GetName().Name))
                {
                    classification = DependencyClassification.Reference;
                }

                return new Dependency(assembly, classification);
            }

            private DependencyClassification ComputeClassification(string dependency)
            {
                if (!_dependencies.ContainsKey(dependency))
                {
                    return DependencyClassification.Unknown;
                }

                var candidateEntry = _dependencies[dependency];
                if (candidateEntry.Classification != DependencyClassification.Unknown)
                {
                    return candidateEntry.Classification;
                }
                else
                {
                    var classification = DependencyClassification.NotCandidate;

                    foreach (var candidateDependency in candidateEntry.Assembly.GetReferencedAssemblies())
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

            public IEnumerable<Assembly> GetCandidates()
            {
                foreach (var dependency in _dependencies)
                {
                    if (ComputeClassification(dependency.Key) == DependencyClassification.Candidate)
                    {
                        yield return dependency.Value.Assembly;
                    }
                }
            }

            private class Dependency
            {
                public Dependency(Assembly assembly, DependencyClassification classification)
                {
                    Assembly = assembly;
                    Classification = classification;
                }

                public Assembly Assembly { get; }
                public DependencyClassification Classification { get; set; }
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