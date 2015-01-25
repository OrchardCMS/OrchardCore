using Microsoft.Data.Entity;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Runtime;
using OrchardVNext.Environment;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions.Loaders;

namespace OrchardVNext.Data
{
    public interface IDataContext {
        DbContext Context { get; }
    }

    public class DataContext : DbContext, IDataContext
    {
        private readonly ShellSettings _shellSettings;
        private readonly IAssemblyProvider _assemblyProvider;

        private readonly Guid _instanceId;

        public DataContext(
            ShellSettings shellSettings, 
            DbContextOptions dbContextOptions,
            IAssemblyProvider assemblyProvider) : base(dbContextOptions) {

            _shellSettings = shellSettings;
            _assemblyProvider = assemblyProvider;
            _instanceId = Guid.NewGuid();

            Configuration.AutoDetectChangesEnabled = true;
        }

        public DbContext Context {
            get {
                return this;
            }
        }

        protected override void OnConfiguring(DbContextOptions options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var entityMethod = modelBuilder.GetType().GetRuntimeMethod("Entity", new Type[] {});

            foreach (var assembly in _assemblyProvider.CandidateAssemblies) {

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
    }
}