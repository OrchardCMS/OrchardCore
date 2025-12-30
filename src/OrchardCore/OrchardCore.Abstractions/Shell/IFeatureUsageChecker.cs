using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell;

public interface IFeatureUsageChecker
{
    Task<bool> IsFeatureInUseAsync(IFeatureInfo feature);
}
