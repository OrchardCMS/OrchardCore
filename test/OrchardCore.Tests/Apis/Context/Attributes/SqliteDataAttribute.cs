using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace OrchardCore.Tests.Apis.Context.Attributes
{
    /// <summary>
    /// Provides a data source for a Sqlite data theory, allowing Sqlite testing to be skipped.
    /// To skip Sqlite testing provide any value to the environment variable 'ORCHARD_TEST_SQLITE_SKIP'
    /// </summary>
    /// <example>
    /// ORCHARD_TEST_SQLITE_SKIP=True
    /// </example>
    /// <remarks>
    /// Primarily used for CI testing.
    /// </remarks>
    public class SqliteDataAttribute : DataAttribute
    {
        private static string Environment = System.Environment.GetEnvironmentVariable("ORCHARD_TEST_SQLITE_SKIP");

        public SqliteDataAttribute()
        {
            if (!String.IsNullOrEmpty(Environment) && String.IsNullOrEmpty(Skip))
            {
                Skip = "Sqlite test skipped by environment";
            }
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
            =>  new object[][]
                {
                    new object[] { "Sqlite", "" }
                };
    }
}
