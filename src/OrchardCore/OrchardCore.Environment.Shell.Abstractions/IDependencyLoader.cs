using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Implementing types perform auto-registration of the specified set of types (typically provided by a shell blueprint).
    /// </summary>
    public interface IDependencyLoader
    {
        /// <summary>
        /// Selects all types that are annotated with <seealso cref="ServiceImplAttribute"/> and registers those types with the specified service container.
        /// </summary>
        /// <param name="types">The un-filtered set of types to look for service types (i.e. types annotated with <seealso cref="ServiceImplAttribute">).</param>
        /// <param name="serviceCollection">The service container to register discovered types with.</param>
        void RegisterDependencies(IEnumerable<Type> types, IServiceCollection serviceCollection);
    }
}