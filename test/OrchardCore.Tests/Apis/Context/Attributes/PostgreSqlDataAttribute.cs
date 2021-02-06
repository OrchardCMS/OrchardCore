using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace OrchardCore.Tests.Apis.Context.Attributes
{
    /// <summary>
    /// Provides a data source for a PostgreSql data theory, enabled via environment variables.
    /// To enable PostgreSql testing provide a valid connection string for the environment variable 'ORCHARD_TEST_POSTGRESQL_CONNECTION_STRING'
    /// </summary>
    /// <example>
    /// ORCHARD_TEST_POSTGRESQL_CONNECTION_STRING=Server=localhost;Port=5432;Database=octest;User Id=postgres;Password=Password12!;
    /// </example>
    /// <remarks>
    /// Primarily used for CI testing.
    /// </remarks>
    public class PostgreSqlDataAttribute : DataAttribute
    {
        private static string Environment = System.Environment.GetEnvironmentVariable("ORCHARD_TEST_POSTGRESQL_CONNECTION_STRING");

        public PostgreSqlDataAttribute()
        {
            if (String.IsNullOrEmpty(Environment) && String.IsNullOrEmpty(Skip))
            {
                Skip = "PostgreSql test skipped by environment";
            }
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
            =>  new object[][]
                {
                    new object[] { "Postgres", Environment }
                };
    }
}
