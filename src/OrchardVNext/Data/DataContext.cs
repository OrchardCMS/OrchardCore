using Microsoft.Data.Entity;
using System;
using Microsoft.Data.Entity.Metadata;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Data
{
    public class DataContext : DbContext
    {
        private readonly ShellSettings _shellSettings;
        public DataContext(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

        }
    }
}