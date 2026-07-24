using System.Runtime.CompilerServices;
using Xunit;

namespace OrchardCore.Tests.Functional.Helpers;

/// <summary>
/// Skips the test when Redis is not configured via environment variable.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RedisFactAttribute : FactAttribute
{
    public RedisFactAttribute([CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(
            "OrchardCore__OrchardCore_Redis__Configuration")))
        {
            Skip = "Redis is not configured. Set OrchardCore__OrchardCore_Redis__Configuration to run this test.";
        }
    }
}

/// <summary>
/// Skips the test when Azurite is not configured via environment variable.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AzuriteFactAttribute : FactAttribute
{
    public AzuriteFactAttribute([CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(
            "OrchardCore__OrchardCore_Media_Azure__ConnectionString")))
        {
            Skip = "Azurite is not configured. Set OrchardCore__OrchardCore_Media_Azure__ConnectionString to run this test.";
        }
    }
}

/// <summary>
/// Skips the test when either Redis or Azurite is not configured.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RedisAndAzuriteFactAttribute : FactAttribute
{
    public RedisAndAzuriteFactAttribute([CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(
                "OrchardCore__OrchardCore_Redis__Configuration")) ||
            string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(
                "OrchardCore__OrchardCore_Media_Azure__ConnectionString")))
        {
            Skip = "Both Redis and Azurite must be configured to run this test.";
        }
    }
}
