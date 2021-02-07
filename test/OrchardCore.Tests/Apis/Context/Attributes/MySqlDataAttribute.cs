using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace OrchardCore.Tests.Apis.Context.Attributes
{
    /// <summary>
    /// Provides a data source for a MySql server data theory, enabled via environment variables.
    /// To enable MySql server testing provide a valid connection string for the environment variable 'ORCHARD_TEST_MYSQL_CONNECTION_STRING'
    /// </summary>
    /// <example>
    /// ORCHARD_TEST_MYSQL_CONNECTION_STRING=server=localhost;uid=root;pwd=Password12!;database=octest;
    /// </example>
    /// <remarks>
    /// Primarily used for CI testing.
    /// </remarks>
    public class MySqlDataAttribute : DataAttribute
    {
        private static string Environment = System.Environment.GetEnvironmentVariable("ORCHARD_TEST_MYSQL_CONNECTION_STRING");

        public MySqlDataAttribute()
        {
            if (String.IsNullOrEmpty(Environment) && String.IsNullOrEmpty(Skip))
            {
                Skip = "MySql test skipped by environment";
            }
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
            =>  new object[][]
                {
                    new object[] { "MySql", Environment }
                };
    }
}
