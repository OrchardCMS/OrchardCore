using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using System.Linq;
using Microsoft.Framework.Logging;

namespace OrchardVNext.Data.EntityFramework {
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

        public override void AddRange([NotNull]IEnumerable<object> entities) {
            base.AddRange(entities);
            SaveChanges();
        }

        public override void AddRange([NotNull]params object[] entities) {
            base.AddRange(entities);
            SaveChanges();
        }

        public override EntityEntry<TEntity> Add<TEntity>([NotNull]TEntity entity) {
            var entry = base.Add<TEntity>(entity);
            SaveChanges();
            return entry;
        }

        public override EntityEntry Add([NotNull]object entity) {
            var entry = base.Add(entity);
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

        //public override int SaveChanges() {
        //    var entriesToSave = ChangeTracker.StateManager.Entries
        //        .Where(e => e.EntityState == EntityState.Added
        //                    || e.EntityState == EntityState.Modified
        //                    || e.EntityState == EntityState.Deleted)
        //        .Select(e => e.PrepareToSave()) // do I need this line!?
        //        .ToList();

        //    // TODO: Call Api to store them in a seperate querying/indexing store i.e. lucene.
        //    // Needs to be async? or place in a messaging queue of some sort.

        //    return base.SaveChanges();
        //}
    }
}