using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Data
{
    public interface IDataContext {
        DataContext Context { get; }
    }

    public class DataContext : DbContext, IDataContext
    {
        private readonly ShellSettings _shellSettings;
        private readonly IAssemblyProvider _assemblyProvider;

        private readonly Guid _instanceId;

        public DataContext(
            ShellSettings shellSettings,
            IDbContextFactoryHolder dbContextFactoryHolder,
            IAssemblyProvider assemblyProvider) : base(dbContextFactoryHolder.BuildConfiguration()) {
            
            _shellSettings = shellSettings;
            _assemblyProvider = assemblyProvider;
            _instanceId = Guid.NewGuid();
        }

        public DataContext Context => this;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var entityMethod = modelBuilder.GetType().GetRuntimeMethod("Entity", new Type[] {});

            foreach (var assembly in _assemblyProvider.CandidateAssemblies) {
                // Keep persistent attribute, but also introduce a convention like ContentPart
                var entityTypes = assembly
                    .GetTypes()
                    .Where(t =>
                        t.GetTypeInfo().GetCustomAttributes<PersistentAttribute>(true)
                            .Any());
                
                foreach (var type in entityTypes) {
                    entityMethod.MakeGenericMethod(type)
                        .Invoke(modelBuilder, new object[] { });
                }
            }
        }

        public override int SaveChanges() {
            var entriesToSave = ChangeTracker.StateManager.StateEntries
                .Where(e => e.EntityState.IsDirty())
                .Select(e => e.PrepareToSave())
                .ToList();

            // TODO: Call Api to store them in a seperate querying/indexing store i.e. lucene.
            // Needs to be async? or place in a messaging queue of some sort.

            return base.SaveChanges();
        }
    }
}