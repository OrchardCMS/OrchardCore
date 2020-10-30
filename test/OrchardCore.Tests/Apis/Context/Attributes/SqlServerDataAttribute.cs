using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace OrchardCore.Tests.Apis.Context.Attributes
{
    /// <summary>
    /// Provides a data source for a Sql Server data theory, enabled via environment variables.
    /// To enable Sql Server testing provide a valid connection string for the environment variable 'ORCHARD_TEST_SQLSERVER_CONNECTION_STRING'
    /// </summary>
    /// <example>
    /// ORCHARD_TEST_SQLSERVER_CONNECTION_STRING=Data Source=.;Initial Catalog=tempdb;User Id=sa;Password=Password12!
    /// </example>
    /// <remarks>
    /// Primarily used for CI testing.
    /// </remarks>
    public class SqlServerDataAttribute : DataAttribute
    {
        private static string Environment = System.Environment.GetEnvironmentVariable("ORCHARD_TEST_SQLSERVER_CONNECTION_STRING");

        public SqlServerDataAttribute()
        {
            if (String.IsNullOrEmpty(Environment) && String.IsNullOrEmpty(Skip))
            {
                Skip = "Sql Server test skipped by environment";
            }
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
            =>  new object[][]
                {
                    new object[] { "SqlConnection", Environment }
                };
    }
}
