using SystemEnvironment = System.Environment;

namespace OrchardCore.Tests;

public class CIFactAttribute : FactAttribute
{
    public override string Skip
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
}
