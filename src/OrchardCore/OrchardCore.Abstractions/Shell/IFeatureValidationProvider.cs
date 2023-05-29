using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Provides validation for whether features are allowed to be disabled on a per tenant basis.
    /// </summary>
    public interface IFeatureValidationProvider
    {
        ValueTask<bool> IsFeatureValidAsync(string id);
    }
}
