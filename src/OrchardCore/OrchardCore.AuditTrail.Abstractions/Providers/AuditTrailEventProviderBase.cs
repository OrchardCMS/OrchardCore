using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Providers
{
    public abstract class AuditTrailEventProviderBase : IAuditTrailEventProvider
    {
        public AuditTrailEventProviderBase(IStringLocalizer s)
        {
            S = s;
        }

        public abstract void Describe(DescribeContext context);
        public IStringLocalizer S { get; }
    }
}
