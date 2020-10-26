using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

#nullable enable
namespace OrchardCore.Tests.Apis.Context.Attributes
{
    /// <summary>
    /// Provides a data source for a MySql server data theory, enabled via environment variables.
    /// To enable MySql server testing provide a valid connection string for the environment variable 'ORCHARD_TEST_MYSQL_CONNECTION_STRING'
    /// </summary>
    /// <remarks>
    /// Primarily used for CI testing.
    /// </remarks>
    public class MySqlDataAttribute : DataAttribute
	{
        // TODO
    // <example>
    //NG=server=localhost;uid=root;pwd=Password12!;database=yessql;
    // ORCHARD_TEST_MYSQL_CONNECTION_STRING='Data Source=.;Initial Catalog=talog=tempdb;User Id=;Password='
    // </example>

        private static string? Environment = System.Environment.GetEnvironmentVariable("ORCHARD_TEST_MYSQL_CONNECTION_STRING");

        public MySqlDataAttribute()
        {
            if (String.IsNullOrEmpty(Environment))
            {
                Skip = "MySql test skipped by environment";
            }
        }

		public override IEnumerable<object?[]> GetData(MethodInfo testMethod)
            =>  new object?[][]
                {
                    new object?[] { "MySql", Environment }
                };
	}
}
#nullable disable
