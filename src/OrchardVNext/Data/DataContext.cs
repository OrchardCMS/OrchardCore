using Microsoft.Data.Entity;
using System;
using Microsoft.Data.Entity.Metadata;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Data
{
    public interface IDataContext : IUnitOfWorkDependency {
        DbContext Context { get; }
    }

    public class DataContext : DbContext, IDataContext
    {
        private readonly ShellSettings _shellSettings;

        private readonly Guid _instanceId;

        public DataContext(ShellSettings shellSettings, DbContextOptions dbContextOptions) : base(dbContextOptions) {
            _shellSettings = shellSettings;
            _instanceId = Guid.NewGuid();

            Configuration.AutoDetectChangesEnabled = true;
        }

        public DbContext Context {
            get {
                return this;
            }
        }

        protected override void OnConfiguring(DbContextOptions options) {
            base.OnConfiguring(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

        }
    }
}