using System.Runtime.CompilerServices;
using SystemEnvironment = System.Environment;

#nullable enable

namespace OrchardCore.Tests;

/// <summary>
/// A test attribute for tests that are only run in Continuous Integration (CI) environments, like GitHub Actions or
/// Azure DevOps.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CIFactAttribute : FactAttribute
{
    public CIFactAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        // "CI" is defined by GitHub Actions.
        // "BUILD_BUILDID" is defined by Azure DevOps.
        if (SystemEnvironment.GetEnvironmentVariable("BUILD_BUILDID") == null &&
            SystemEnvironment.GetEnvironmentVariable("CI") == null)
        {
            Skip = $"{nameof(CIFactAttribute)} tests are not run locally. To run them locally create a \"CI\" environment variable.";
        }
    }
}
