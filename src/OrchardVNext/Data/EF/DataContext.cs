using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Data.EF {
    public interface IDataContext {
        DataContext Context { get; }
    }

    public class DataContext : DbContext, IDataContext {
        private readonly ShellSettings _shellSettings;
        private readonly IDbContextFactoryHolder _dbContextFactoryHolder;
        private readonly IAssemblyProvider _assemblyProvider;

        private readonly Guid _instanceId;

        public DataContext(
            ShellSettings shellSettings,
            IDbContextFactoryHolder dbContextFactoryHolder,
            IAssemblyProvider assemblyProvider) {

            _shellSettings = shellSettings;
            _dbContextFactoryHolder = dbContextFactoryHolder;
            _assemblyProvider = assemblyProvider;
            _instanceId = Guid.NewGuid();
        }

        public DataContext Context => this;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var entityMethod = modelBuilder.GetType().GetRuntimeMethod("Entity", new Type[0]);

            foreach (var assembly in _assemblyProvider.CandidateAssemblies.Distinct()) {
                // Keep persistent attribute, but also introduce a convention like ContentPart
                var entityTypes = assembly
                    .GetTypes()
                    .Where(t =>
                        t.GetTypeInfo().GetCustomAttributes<PersistentAttribute>(true)
                            .Any());

                foreach (var type in entityTypes) {
                    Logger.Debug("Mapping record {0}", type.FullName);

                    entityMethod.MakeGenericMethod(type)
                        .Invoke(modelBuilder, new object[0]);
                }
            }
        }

        protected override void OnConfiguring(DbContextOptions options) {
            _dbContextFactoryHolder.Configure(options);
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