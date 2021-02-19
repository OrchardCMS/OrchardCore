using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Data
{
    public static class StoreCollectionOptionsServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a <see cref="Type"/> as supporting collections.
        /// This is an internal Orchard Core validation that the backing store has support for collections.
        /// </summary>
        public static IServiceCollection AddCollectionSupport(this IServiceCollection services, Type type)
            => services.Configure<StoreCollectionOptions>(o => o.AddCollectionSupport(type));

        /// <summary>
        /// Registers a <see cref="Type"/> as supporting collections.
        /// This is an internal Orchard Core validation that the backing store has support for collections.
        /// </summary>
        public static IServiceCollection AddCollectionSupport<T>(this IServiceCollection services) where T : class
            => services.AddCollectionSupport(typeof(T));

        /// <summary>
        /// Specifies a <see cref="Type"/> with a collection name.
        /// This must be called before the site has been setup and migrations have been run.
        /// To enable a collection after the related index tables have been created, the index tables must be renamed manually. 
        /// Registering a <see cref="Type"/> for a backing store that does not support collections will result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        public static IServiceCollection WithCollection(this IServiceCollection services, Type type, string collection)
            => services.Configure<StoreCollectionOptions>(o => o.WithCollection(type, collection));

        /// <summary>
        /// Specifies a <see cref="Type"/> with a collection name.
        /// This must be called before the site has been setup and migrations have been run.
        /// To enable a collection after the related index tables have been created, the index tables must be renamed manually. 
        /// Registering a <see cref="Type"/> for a backing store that does not support collections will result in an <see cref="InvalidOperationException"/>.
        /// </summary>
        public static IServiceCollection WithCollection<T>(this IServiceCollection services, string collection) where T : class
            => services.Configure<StoreCollectionOptions>(o => o.WithCollection(typeof(T), collection));
    }
}
