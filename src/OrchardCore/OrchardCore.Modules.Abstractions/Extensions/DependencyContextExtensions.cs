using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyModel;

namespace OrchardCore.Modules
{
    public static class DependencyContextExtensions
    {
        /// <summary>
        /// Returns libraries which are not included in 'references' but depend on one of them.
        /// </summary>
        public static IEnumerable<RuntimeLibrary> GetCandidateLibraries(
            this DependencyContext dependencyContext, IEnumerable<string> references)
        {
            if (references == null)
            {
                return Enumerable.Empty<RuntimeLibrary>();
            }

            return new CandidateResolver(dependencyContext.RuntimeLibraries,
                new HashSet<string>(references)).GetCandidates();
        }

        private class CandidateResolver
        {
            private readonly IDictionary<string, Dependency> _runtimeDependencies;

            public CandidateResolver(IReadOnlyList<RuntimeLibrary> runtimeDependencies, ISet<string> references)
            {
                var dependenciesWithNoDuplicates = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);
                foreach (var dependency in runtimeDependencies)
                {
                    if (dependenciesWithNoDuplicates.ContainsKey(dependency.Name))
                    {
                        throw new InvalidOperationException($"A duplicate entry for library reference {dependency.Name} was found.");
                    }

                    dependenciesWithNoDuplicates.Add(dependency.Name, CreateDependency(dependency, references));
                }

                _runtimeDependencies = dependenciesWithNoDuplicates;
            }

            private Dependency CreateDependency(RuntimeLibrary library, ISet<string> references)
            {
                var classification = DependencyClassification.Unknown;
                if (references.Contains(library.Name))
                {
                    classification = DependencyClassification.Reference;
                }

                return new Dependency(library, classification);
            }

            private DependencyClassification ComputeClassification(string dependency)
            {
                if (!_runtimeDependencies.ContainsKey(dependency))
                {
                    // Library does not have runtime dependency. Since we can't infer
                    // anything about it's references, we'll assume it does not have a reference to.
                    return DependencyClassification.DoesNotReference;
                }

                var candidateEntry = _runtimeDependencies[dependency];
                if (candidateEntry.Classification != DependencyClassification.Unknown)
                {
                    return candidateEntry.Classification;
                }
                else
                {
                    var classification = DependencyClassification.DoesNotReference;
                    foreach (var candidateDependency in candidateEntry.Library.Dependencies)
                    {
                        var dependencyClassification = ComputeClassification(candidateDependency.Name);
                        if (dependencyClassification == DependencyClassification.References ||
                            dependencyClassification == DependencyClassification.Reference)
                        {
                            classification = DependencyClassification.References;
                            break;
                        }
                    }

                    candidateEntry.Classification = classification;

                    return classification;
                }
            }

            public IEnumerable<RuntimeLibrary> GetCandidates()
            {
                foreach (var dependency in _runtimeDependencies)
                {
                    if (ComputeClassification(dependency.Key) == DependencyClassification.References)
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

                /// <summary>
                /// References (directly or transitively).
                /// </summary>
                References = 1,

                /// <summary>
                /// Does not reference (directly or transitively).
                /// </summary>
                DoesNotReference = 2,

                /// <summary>
                /// One of the references.
                /// </summary>
                Reference = 3,
            }
        }
    }
}