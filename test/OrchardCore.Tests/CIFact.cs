using Xunit.v3;
using SystemEnvironment = System.Environment;

#nullable enable

namespace OrchardCore.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CIFactAttribute : Attribute, IFactAttribute
{
    /// <inheritdoc/>
    public string? DisplayName { get; set; }

    /// <inheritdoc/>
    public bool Explicit { get; set; }

    /// <inheritdoc/>
    public string? Skip
    {
        get
        {
            // "CI" is defined by GitHub actions
            // "BUILD_BUILDID" is defined by Azure DevOps
            if (SystemEnvironment.GetEnvironmentVariable("BUILD_BUILDID") == null &&
                SystemEnvironment.GetEnvironmentVariable("CI") == null)
            {
                return $"{nameof(CIFactAttribute)} tests are not run locally. To run them locally create a \"CI\" environment variable.";
            }

            return null!;
        }
    }

    /// <inheritdoc/>
    public Type? SkipType { get; set; }

    /// <inheritdoc/>
    public string? SkipUnless { get; set; }

    /// <inheritdoc/>
    public string? SkipWhen { get; set; }

    /// <inheritdoc/>
    public int Timeout { get; set; }
}
