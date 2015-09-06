using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.Entity;
using System.Linq;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;

namespace Orchard.Data.EntityFramework {
    public interface IDataContext {
        DataContext Context { get; }
    }

    public class DataContext : DbContext, IDataContext {
        private readonly IDbContextFactoryHolder _dbContextFactoryHolder;
        private readonly IOrchardDataAssemblyProvider _assemblyProvider;
        private readonly ILogger _logger;

        private readonly Guid _instanceId;

        public DataContext(
            IDbContextFactoryHolder dbContextFactoryHolder,
            IOrchardDataAssemblyProvider assemblyProvider,
            ILoggerFactory loggerFactory) {
            
            _dbContextFactoryHolder = dbContextFactoryHolder;
            _assemblyProvider = assemblyProvider;
            _logger = loggerFactory.CreateLogger<DataContext>();
            _instanceId = Guid.NewGuid();
        }

        public DataContext Context => this;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            _logger.LogInformation("[{0}]: Mapping Records to DB Context", GetType().Name);
            var sw = Stopwatch.StartNew();

            var entityMethod = modelBuilder.GetType().GetRuntimeMethod("Entity", new Type[0]);

            foreach (var assembly in _assemblyProvider.CandidateAssemblies.Distinct()) {
                // Keep persistent attribute, but also introduce a convention like ContentPart
                var entityTypes = assembly
                    .ExportedTypes
                    .Where(t =>
                        typeof(StorageDocument).IsAssignableFrom(t) &&
                        !t.GetTypeInfo().IsAbstract && !t.GetTypeInfo().IsInterface);

                foreach (var type in entityTypes) {
                    _logger.LogDebug("Mapping record {0}", type.FullName);

                    entityMethod.MakeGenericMethod(type)
                        .Invoke(modelBuilder, new object[0]);
                }
            }

            sw.Stop();
            _logger.LogInformation("[{0}]: Records Mapped in {1}ms", GetType().Name, sw.ElapsedMilliseconds);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            _dbContextFactoryHolder.Configure(optionsBuilder);
        }

		
        public override void AddRange([NotNull]IEnumerable<object> entities, bool includeDependents = true) {		
            base.AddRange(entities, includeDependents);		
            SaveChanges();		
        }		
		
        public override void AddRange([NotNull]params object[] entities) {		
            base.AddRange(entities);		
            SaveChanges();		
        }
        
        public override EntityEntry<TEntity> Add<TEntity>([NotNull]TEntity entity, bool includeDependents = true) {		
            var entry = base.Add<TEntity>(entity, includeDependents);		
            SaveChanges();		
            return entry;		
        }		
		
        public override EntityEntry Add([NotNull]object entity, bool includeDependents = true) {		
            var entry = base.Add(entity, includeDependents);		
            SaveChanges();		
            return entry;		
        }		
		
        public override EntityEntry<TEntity> Remove<TEntity>([NotNull]TEntity entity) {		
            var entry = base.Remove<TEntity>(entity);		
            SaveChanges();		
            return entry;		
        }		
		
        public override EntityEntry Remove([NotNull]object entity) {		
            var entry = base.Remove(entity);		
            SaveChanges();		
            return entry;		
        }
    }
}