using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Extensions.Features.Attributes;

namespace OrchardCore.Environment.Shell
{
    public class DependencyLoader : IDependencyLoader
    {
        public void RegisterDependencies(IEnumerable<Type> types, IServiceCollection serviceCollection)
        {
            // Discover dependencies to be registered.
            // Only types annotated with either [ServiceImpl] or [ServiceOverride] attributes are selected.
            var dependenciesQuery =
                from dependency in types
                let dependencyTypeInfo = dependency.GetTypeInfo()
                let hasDependencyAttribute = dependencyTypeInfo.GetCustomAttribute<ServiceImplAttribute>() != null
                where hasDependencyAttribute && dependencyTypeInfo.IsClass && !dependencyTypeInfo.IsAbstract
                select new DependencyInfo(dependency, dependencyTypeInfo);

            var filteredDependencies = FilterSuppressedTypes(dependenciesQuery).ToList();

            // For each dependency, select its service type(s) to be registered as.
            foreach (var dependencyInfo in filteredDependencies)
            {
                var dependency = dependencyInfo.DependencyType;
                var dependencyTypeInfo = dependencyInfo.DependencyTypeInfo;
                var typesToCheck = (new[] { dependency, dependencyTypeInfo.BaseType }).Concat(dependencyTypeInfo.ImplementedInterfaces).ToList();
                var serviceTypeInfos =
                    (from type in typesToCheck
                     let typeInfo = type.GetTypeInfo()
                     let serviceAttributes = typeInfo.GetCustomAttributes<ServiceAttribute>()
                     from serviceAttribute in serviceAttributes
                     select new
                     {
                         Type = type,
                         TypeInfo = typeInfo,
                         ServiceAttribute = serviceAttribute
                     }).ToList();

                // Register the dependency by each of its declared service types.
                foreach (var serviceTypeInfo in serviceTypeInfos)
                {
                    var serviceType = serviceTypeInfo.ServiceAttribute.ServiceType ?? serviceTypeInfo.Type;

                    switch (serviceTypeInfo.ServiceAttribute.ServiceLifetime)
                    {
                        case ServiceLifetime.Transient:
                            serviceCollection.AddTransient(serviceType, dependency);
                            break;
                        case ServiceLifetime.Singleton:
                            serviceCollection.AddSingleton(serviceType, dependency);
                            break;
                        case ServiceLifetime.Scoped:
                        default:
                            serviceCollection.AddScoped(serviceType, dependency);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Filters out any types that have been overridden using the <seealso cref="ServiceOverrideAttribute"/>.
        /// </summary>
        private IEnumerable<DependencyInfo> FilterSuppressedTypes(IEnumerable<DependencyInfo> dependencies)
        {
            var excludedTypesQuery =
                from dependency in dependencies
                let typeInfo = dependency.DependencyType.GetTypeInfo()
                let replacedType = typeInfo.GetCustomAttribute<ServiceOverrideAttribute>()
                where replacedType != null
                select replacedType.TypeName;

            var excludedTypes = excludedTypesQuery.ToList();
            return dependencies.Where(x => !excludedTypes.Contains(x.DependencyType.FullName));
        }

        private class DependencyInfo
        {
            public DependencyInfo(Type dependencyType, TypeInfo typeInfo)
            {
                DependencyType = dependencyType;
                DependencyTypeInfo = typeInfo;
            }

            public Type DependencyType { get; }
            public TypeInfo DependencyTypeInfo { get; }
        }
    }
}