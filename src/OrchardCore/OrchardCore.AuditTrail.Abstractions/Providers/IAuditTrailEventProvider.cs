using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Providers
{
    /// <summary>
    /// Audit Trail Event Provider implementations can describe events contributing to Audit Trail.
    /// </summary>
    public interface IAuditTrailEventProvider
    {
        /// <summary>
        /// Describe the category and the context used to build a custom Audit Trail event.
        /// </summary>
        void Describe(DescribeContext context);
    }
}
