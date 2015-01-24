using Microsoft.Data.Entity;
using System;
using System.Data.Common;

namespace OrchardVNext.Data.Providers {
    public interface IDataServicesProvider : ITransientDependency {
        string ProviderName { get; }
        DbContextOptions BuildContextOptions();
    }

    public class SqlServerDataServicesProvider : IDataServicesProvider {
        public string ProviderName {
            get { return "SqlServer"; }
        }

        public DbContextOptions BuildContextOptions() {



            DbContextOptions foo = new DbContextOptions();
            foo.UseSqlServer(@"");
            return foo;
        }
    }
}